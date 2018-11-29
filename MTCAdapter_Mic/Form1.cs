// MTConnect Adapter to send audio summation only
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MTConnect;
using System.Threading;
using XMLGathering;

using MTC_adapter_daq;


// 8000, Mono
// 10sec = 3535114 bytes

namespace MTCAdapter_Mic
{
    public partial class MicAdapter : Form
    {
        
        // Mic devices
        private IWaveIn captureDevice;
        private IWaveIn captureDevice2;

        private WaveFileWriter writer;
        private WaveFileWriter writer2;

        private string outputFilename;
        private string outputFilename2;

        private string outputFolder;
        private string baseFolder;


        // NI DAQ device
        private DAQHandler daq;
        private const int samplingRate = 1000;
        private const int samplingSec = 30;

        // MTConnect - common
        private const int MTC_port_num = 7877;
        private bool isMTconnected = false;

        // MTConnect - writing stethoscope
        Adapter mAdapter = new Adapter();        
        MTConnect.Sample sample_signal_sum = new Sample("audio_signal1");
        MTConnect.Event audio_filename = new Event("audio_rec_filename");

        // MTConnect - reading Kuka robot 
        MTCXMLParserKuka_forAudio mKuka = new MTCXMLParserKuka_forAudio();
        static String kukaBaseUrl = "http://128.46.131.12/KUKA_Robot";
        static String kukaCurrentUrl = kukaBaseUrl + "/current";
        int sec_start = 0;
        int sec_end = 0;

        // Auto calculation
        private double runInterval = 0.2; // timer1 loop (sec)       


        // Folder numbering
        private static int folderNum = 1;



        public MicAdapter()
        {
            // MTConnect initialization
            // Initialization for MTConnect

            mAdapter.AddDataItem(sample_signal_sum);
            mAdapter.AddDataItem(audio_filename);
            InitializeComponent();
            LoadWasapiDevicesCombo();

            comboBoxSampleRate.DataSource = new[] { 8000, 16000, 22050, 32000, 44100, 48000 };
            comboBoxSampleRate.SelectedIndex = 0;
            comboBoxChannels.DataSource = new[] { "Mono", "Stereo" };
            comboBoxChannels.SelectedIndex = 0;
            outputFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            baseFolder = Directory.GetCurrentDirectory();
            outputFolder = baseFolder + "/" + folderNum;
            //Directory.CreateDirectory(outputFolder);
            //outputFilename = GetFileName();

            //timer1.Interval = (int)(runInterval * 1000);  

        }

        private string GetFileName()
        {
            var deviceName = captureDevice.GetType().Name;
            var sampleRate = $"{captureDevice.WaveFormat.SampleRate / 1000}kHz";
            var channels = captureDevice.WaveFormat.Channels == 1 ? "mono" : "stereo";

            return $"{deviceName} {sampleRate} {channels} {DateTime.Now:yyy-MM-dd HH-mm-ss}.wav";
            //return "test.wav";
        }

        private string GetFileName2()
        {
            var deviceName = captureDevice2.GetType().Name;
            var sampleRate = $"{captureDevice2.WaveFormat.SampleRate / 1000}kHz";
            var channels = captureDevice2.WaveFormat.Channels == 1 ? "mono" : "stereo";

            return $"{deviceName}2 {sampleRate} {channels} {DateTime.Now:yyy-MM-dd HH-mm-ss}.wav";
            //return "test.wav";
        }



        private void LoadWasapiDevicesCombo()
        {
            var deviceEnum = new MMDeviceEnumerator();
            var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            var deviceEnum2 = new MMDeviceEnumerator();
            var devices2 = deviceEnum2.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            comboWasapiDevices.DataSource = devices;
            comboWasapiDevices.DisplayMember = "FriendlyName";

            comboWasapiDevices2.DataSource = devices2;
            comboWasapiDevices2.DisplayMember = "FriendlyName";

        }
        

        private IWaveIn CreateWaveInDevice()
        {
            IWaveIn newWaveIn;

            // can't set WaveFormat as WASAPI doesn't support SRC
            var device = (MMDevice)comboWasapiDevices.SelectedItem;
            newWaveIn = new WasapiCapture(device);

            newWaveIn.DataAvailable += OnDataAvailable;
            newWaveIn.RecordingStopped += OnRecordingStopped;
            return newWaveIn;
        }

        private IWaveIn CreateWaveInDevice2()
        {
            IWaveIn newWaveIn;

            // can't set WaveFormat as WASAPI doesn't support SRC
            var device = (MMDevice)comboWasapiDevices2.SelectedItem;
            newWaveIn = new WasapiCapture(device);

            newWaveIn.DataAvailable += OnDataAvailable2;
            newWaveIn.RecordingStopped += OnRecordingStopped2;
            return newWaveIn;
        }


        void OnDataAvailable(object sender, WaveInEventArgs e)
        {

            if (InvokeRequired)
            {
                //Debug.WriteLine("Data Available");
                BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
            }
            else
            {
                //Debug.WriteLine("Flushing Data Available");
                if (writer != null)
                {
                    writer.Write(e.Buffer, 0, e.BytesRecorded);
                    int secondsRecorded = (int)(writer.Length / writer.WaveFormat.AverageBytesPerSecond);
                    if (secondsRecorded >= 30)
                    {
                        StopRecording();
                    }
                    else
                    {
                        progressBar1.Value = secondsRecorded;
                    }
                }
                
            }

        }


        void OnDataAvailable2(object sender, WaveInEventArgs e)
        {

            if (InvokeRequired)
            {
                //Debug.WriteLine("Data Available");
                BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable2), sender, e);
            }
            else
            {
                //Debug.WriteLine("Flushing Data Available");
                if(writer2!=null)
                {
                    writer2.Write(e.Buffer, 0, e.BytesRecorded);
                    int secondsRecorded = (int)(writer2.Length / writer.WaveFormat.AverageBytesPerSecond);
                    if (secondsRecorded >= 30)
                    {
                        StopRecording();
                    }
                    else
                    {
                        progressBar1.Value = secondsRecorded;
                    }
                }
                    
            }

        }


        private void SetControlStates(bool isRecording)
        {
           // groupBoxRecordingApi.Enabled = !isRecording;
            btnStartRecording.Enabled = !isRecording;
            buttonStopRecording.Enabled = isRecording;
        }


        private void FinalizeWaveFile()
        {
            
            
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(outputFolder);

            Debug.WriteLine("Starting Mic acquisition..");
            runRecordWAV();


            Debug.WriteLine("Starting DAQ acquisition..");
            daq = new DAQHandler(samplingRate);            

        }

        private void runRecordWAV()
        {
            SetControlStates(true);

            if(comboWasapiDevices.Text == comboWasapiDevices2.Text)
            {
                MessageBox.Show("Choosed the same devices. Try different devices.");
                SetControlStates(false);
                return;
            }            

            captureDevice = CreateWaveInDevice();                        
            var device = (MMDevice)comboWasapiDevices.SelectedItem; // Forcibly turn on the microphone (some programs (Skype) turn it off).
            device.AudioEndpointVolume.Mute = false;
            outputFilename = GetFileName();            
            writer = new WaveFileWriter(Path.Combine(outputFolder, outputFilename), captureDevice.WaveFormat);            
            captureDevice.StartRecording();


            if(chk2ndAvail.Checked)
            {
                captureDevice2 = CreateWaveInDevice2();
                var device2 = (MMDevice)comboWasapiDevices2.SelectedItem; // Forcibly turn on the microphone (some programs (Skype) turn it off).
                device2.AudioEndpointVolume.Mute = false;
                outputFilename2 = GetFileName2();
                writer2 = new WaveFileWriter(Path.Combine(outputFolder, outputFilename2), captureDevice2.WaveFormat);                
                captureDevice2.StartRecording();
            }

            txtStatus.AppendText("Recoding started\n");
            txtStatus.Update();

            // Write MTConnect stethoscope
            mAdapter.Begin();
            audio_filename.Value = outputFilename;
            mAdapter.SendChanged();

            // Remember starting point of MTConnect of Kuka robot
            mKuka.readCurrentSeq(kukaCurrentUrl);
            sec_start=mKuka.lastSeq;

            //timer1.Start();   // Do not run timer when amplitude signals are not used.


        }


        void StopRecording()
        {
            Debug.WriteLine("StopRecoding() called..");
            captureDevice?.StopRecording();
            captureDevice2?.StopRecording();
            //signal_sum = 0;
            Thread.Sleep(101);
            //timer1.Stop();

            // Remember end point of MTConnect of Kuka robot
            mKuka.readCurrentSeq(kukaCurrentUrl);
            sec_end = mKuka.lastSeq;

        }

        private void buttonStopRecording_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Stop recoding clicked..");

            Debug.WriteLine("Saving mic..");            
            
            StopRecording();

            recordKukaCSV();


            Debug.WriteLine("Saving DAQ..");
            //daq.measureDAQ();
            daq.stopDAQ();

            string fileName = outputFilename;
            fileName = fileName.Remove(fileName.Length - 3, 3);
            fileName += "xml";
            daq.saveDataToExcel(Path.Combine(outputFolder, fileName));


            folderNum++;
            outputFolder = baseFolder + "/" + folderNum;
            

        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("OnRecordingStopped() called");
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<StoppedEventArgs>(OnRecordingStopped), sender, e);
            }
            else
            {
                writer?.Dispose();
                writer = null;

                progressBar1.Value = 0;
                if (e.Exception != null)
                {
                    MessageBox.Show(String.Format("A problem was encountered during recording {0}",
                                                  e.Exception.Message));
                }
                int newItemIndex = listBoxRecordings.Items.Add(outputFilename);                
                listBoxRecordings.SelectedIndex = newItemIndex;
                SetControlStates(false);                
                txtStatus.AppendText("Recoding stopped\n");
                txtStatus.Update();
            }
        }


        void OnRecordingStopped2(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("OnRecordingStopped2() called");
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<StoppedEventArgs>(OnRecordingStopped2), sender, e);
            }
            else
            {
                writer2?.Dispose();
                writer2 = null;                
                
                if (e.Exception != null)
                {
                    MessageBox.Show(String.Format("A problem was encountered during recording {0}",
                                                  e.Exception.Message));
                }

                int newItemIndex = listBoxRecordings.Items.Add(outputFilename2);
                listBoxRecordings.SelectedIndex = newItemIndex;
                
                txtStatus.AppendText("Recoding2 stopped\n");
                txtStatus.Update();
            }
        }


        /// <summary>
        /// Record every value of KUKA robot until the recording is finished...
        /// </summary>
        private void recordKukaCSV()
        {


            string url = kukaBaseUrl + "/sample?from=" + sec_start + "&count=" + (sec_end-sec_start+1);
            Debug.WriteLine(url);

            // read XML - MTConnect sample
            mKuka.readPositionFromCurrent(kukaBaseUrl + "/current");    // To check positions 
            mKuka.readFromSample(url);

            // write to CSV
            try
            {

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(outputFolder, outputFilename + ".csv"), false))  // do not append
                {

                    file.WriteLine("Seq,Timestamp,To_sec,name,value");
                    for (int i = 0; i<6; i++)
                    {

                        foreach(MTCValue mMTCValue in mKuka.list_angles[i])
                        {
                            file.WriteLine(mMTCValue.idx + "," + mMTCValue.tStamp + "," + mMTCValue.toSec() + 
                                "," + mMTCValue.name + "," + mMTCValue.value);
                        }
                        
                    }

                    for (int i = 0; i < 6; i++)
                    {

                        foreach (MTCValue mMTCValue in mKuka.list_torques[i])
                        {
                            file.WriteLine(mMTCValue.idx + "," + mMTCValue.tStamp + "," + mMTCValue.toSec() +
                                "," + mMTCValue.name + "," + mMTCValue.value);
                        }

                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught at writing CSV.", e);
            }




        }
        private void btnMTConnect_Click(object sender, EventArgs e)
        {
            // Start MTConnect agent
            if(!isMTconnected)
            {
                mAdapter.Port = MTC_port_num;    // port number
                mAdapter.Start();
                btnMTConnect.Text = "Disconnect MTC";
                isMTconnected = true;
            } else
            {
                mAdapter.Stop();
                btnMTConnect.Text = "Connect MTC";
                isMTconnected = false;
            }
        }     

        private void timer1_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("timer1 called");
            

            
        }

        private void listBoxRecordings_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textInterval_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboWasapiDevices_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MicAdapter_Load(object sender, EventArgs e)
        {

        }

        private void comboWasapiDevices2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("timer2 called");            
            
            
        }
    }
}
