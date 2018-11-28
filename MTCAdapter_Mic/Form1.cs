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


// 8000, Mono
// 10sec = 3535114 bytes




namespace MTCAdapter_Mic
{
    public partial class MicAdapter : Form
    {
        private const int MTC_port_num = 7877;
        private bool isMTconnected = false;

        private IWaveIn captureDevice;
        private WaveFileWriter writer;
        private string outputFilename;
        private readonly string outputFolder;

        
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
            outputFolder = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(outputFolder);
            //outputFilename = GetFileName();

            timer1.Interval = (int)(runInterval * 1000);  

        }

        private string GetFileName()
        {
            var deviceName = captureDevice.GetType().Name;
            var sampleRate = $"{captureDevice.WaveFormat.SampleRate / 1000}kHz";
            var channels = captureDevice.WaveFormat.Channels == 1 ? "mono" : "stereo";

            return $"{deviceName} {sampleRate} {channels} {DateTime.Now:yyy-MM-dd HH-mm-ss}.wav";
            //return "test.wav";
        }

        private void LoadWasapiDevicesCombo()
        {
            var deviceEnum = new MMDeviceEnumerator();
            var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            comboWasapiDevices.DataSource = devices;
            comboWasapiDevices.DisplayMember = "FriendlyName";

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

        

        private void SetControlStates(bool isRecording)
        {
           // groupBoxRecordingApi.Enabled = !isRecording;
            btnStartRecording.Enabled = !isRecording;
            buttonStopRecording.Enabled = isRecording;
        }


        private void FinalizeWaveFile()
        {
            writer?.Dispose();
            writer = null;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("buttonPlay clicked");
            txtStatus.AppendText("Recoding started\n");
            txtStatus.Update();

            runRecordWAV();

        }

        private void runRecordWAV()
        {
            SetControlStates(true);
            captureDevice = CreateWaveInDevice();
            //if (captureDevice == null)
            //{
            //    captureDevice = CreateWaveInDevice();

            //}

            // Forcibly turn on the microphone (some programs (Skype) turn it off).
            var device = (MMDevice)comboWasapiDevices.SelectedItem;
            device.AudioEndpointVolume.Mute = false;

            outputFilename = GetFileName();
            writer = new WaveFileWriter(Path.Combine(outputFolder, outputFilename), captureDevice.WaveFormat);
            captureDevice.StartRecording();

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
            Debug.WriteLine("Stopped Recording");
            captureDevice?.StopRecording();
            //signal_sum = 0;
            Thread.Sleep(101);
            //timer1.Stop();

            // Remember end point of MTConnect of Kuka robot
            mKuka.readCurrentSeq(kukaCurrentUrl);
            sec_end = mKuka.lastSeq;


        }

        private void buttonStopRecording_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<StoppedEventArgs>(OnRecordingStopped), sender, e);
            }
            else
            {
                FinalizeWaveFile();
                recordKukaCSV();
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

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFilename + ".csv", false))  // do not append
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
    }
}
