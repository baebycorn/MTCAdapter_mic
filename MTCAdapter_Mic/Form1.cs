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

using AForge.Math;
using MTConnect;
using System.Threading;
// 8000, Mono
// 10sec = 3535114 bytes




namespace MTCAdapter_Mic
{
    public partial class MicAdapter : Form
    {
        private const int MTC_port_num = 7877;
        private bool isMTconnected = false;

        private IWaveIn captureDevice;
        //private WaveFileWriter writer;
        //private string outputFilename;
        //private readonly string outputFolder;

        // Globals: MTConnect
        Adapter mAdapter = new Adapter();

        //MTConnect.TimeSeries FFT_time = new TimeSeries("fft_f");
        //MTConnect.TimeSeries FFT_ts = new TimeSeries("fft_v");
        MTConnect.Sample sample_signal_sum = new Sample("audio_signal1");

        // Auto calculation
        private double runInterval = 0.1; // timer1 loop (sec)
        private bool isAuto = false;
        private float signal_sum = 0;

        public struct XY_Signal
        {
            public double x;
            public double y;
            public XY_Signal(double a, double b)
            {
                x = a;
                y = b;
            }

        }

        public MicAdapter()
        {

            // MTConnect initialization
            // Initialization for MTConnect
            //mAdapter.AddDataItem(sample_signal_sum);            


            
            InitializeComponent();
            LoadWasapiDevicesCombo();

            comboBoxSampleRate.DataSource = new[] { 8000, 16000, 22050, 32000, 44100, 48000 };
            comboBoxSampleRate.SelectedIndex = 0;
            comboBoxChannels.DataSource = new[] { "Mono", "Stereo" };
            comboBoxChannels.SelectedIndex = 0;
            //outputFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            //outputFolder = Directory.GetCurrentDirectory();
            //Directory.CreateDirectory(outputFolder);
            //outputFilename = GetFileName();

            timer1.Interval = (int)(runInterval * 1000);  


        }

        //private string GetFileName()
        //{
        //    //var deviceName = captureDevice.GetType().Name;
        //    //var sampleRate = $"{captureDevice.WaveFormat.SampleRate / 1000}kHz";
        //    //var channels = captureDevice.WaveFormat.Channels == 1 ? "mono" : "stereo";

        //    //return $"{deviceName} {sampleRate} {channels} {DateTime.Now:yyy-MM-dd HH-mm-ss}.wav";
        //    return "test.wav";
        //}

        private void LoadWasapiDevicesCombo()
        {
            var deviceEnum = new MMDeviceEnumerator();
            var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            comboWasapiDevices.DataSource = devices;
            comboWasapiDevices.DisplayMember = "FriendlyName";

            //var renderDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            //comboWasapiLoopbackDevices.DataSource = renderDevices;
            //comboWasapiLoopbackDevices.DisplayMember = "FriendlyName";

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

                
                for (int i=0; i<e.Buffer.Length; i++)
                {
                    if (i % 8 != 0)     // Mono but stereo buffer, so only one channel is selected.
                        continue;

                    byte[] tmp = new byte[4];
                    for (int j = 0; j < 4; j++)
                        tmp[j] = e.Buffer[i + j];

                    signal_sum += BitConverter.ToSingle(tmp, 0);   // Calculate the sum
                }             

                //Debug.WriteLine("e buffer size={0}", e.Buffer.Length);
                //Debug.WriteLine("signal_sum={0}", signal_sum);
            



                //int secondsRecorded = (int)(writer.Length / writer.WaveFormat.AverageBytesPerSecond);
                //if (secondsRecorded >= 1)
                //{
                //    StopRecording();
                //}
                //else
                //{
                //    progressBar1.Value = secondsRecorded;
                //}
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
            //writer?.Dispose();
            //writer = null;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("buttonPlay clicked");


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



            //outputFilename = GetFileName();
            //writer = new WaveFileWriter(Path.Combine(outputFolder, outputFilename), captureDevice.WaveFormat);
            captureDevice.StartRecording();

            timer1.Start();

            //SetControlStates(true);
        }


        void StopRecording()
        {
            Debug.WriteLine("Stopped Recording");
            captureDevice?.StopRecording();
            signal_sum = 0;
            Thread.Sleep(101);
            timer1.Stop();
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
                progressBar1.Value = 0;
                if (e.Exception != null)
                {
                    MessageBox.Show(String.Format("A problem was encountered during recording {0}",
                                                  e.Exception.Message));
                }
                //int newItemIndex = listBoxRecordings.Items.Add(outputFilename);
                //listBoxRecordings.SelectedIndex = newItemIndex;
                SetControlStates(false);
                //runFFT();
                //Debug.Write("Recoding+FFT stopped");
                txtStatus.AppendText("Recoding stopped"); txtStatus.Update();

            }
        }

        private void runFFT()
        {
            //// Prepare to read WAV file
            ////System.IO.FileStream WaveFile = System.IO.File.OpenRead(Path.Combine(outputFolder, outputFilename));
            //byte[] data = new byte[WaveFile.Length];
            //WaveFile.Read(data, 0, Convert.ToInt32(WaveFile.Length));
            //Debug.WriteLine("Wave length=", WaveFile.Length);
            //List<byte> data_list = new List<byte>();
            //data_list.AddRange(data);
            //WaveFile.Close();

            //// remove WAV header            
            //List<byte> data_part = data_list.GetRange(58, data_list.Count - 58);

            //// Read WAV data
            //List<float> waves = new List<float>();  // final wave value
            //for (int i = 0; i < data_part.Count; i = i + 8)
            //{
            //    byte[] tmp_l = data_part.GetRange(i, 4).ToArray();
            //    float tmp_lf = BitConverter.ToSingle(tmp_l, 0);
            //    byte[] tmp_r = data_part.GetRange(i + 4, 4).ToArray();
            //    float tmp_rf = BitConverter.ToSingle(tmp_r, 0);
            //    waves.Add(tmp_lf);  // Calculated both, but used one side
            //}

            //// WAVE data to complex value
            //List<AForge.Math.Complex> waves_complex = new List<Complex>();

            //for (int i = 0; i < waves.Count; i++)
            //{
            //    Complex tmp;
            //    tmp.Re = (double)waves.ElementAt(i);
            //    tmp.Im = 0;
            //    waves_complex.Add(tmp);
            //}

            //const int length_FFT = 8192;
            //List<AForge.Math.Complex> waves_complex_part = waves_complex.GetRange(0, length_FFT);

            //Complex[] results = waves_complex_part.ToArray();

            //// Perform FFT. results are saved in reasults itself...
            //AForge.Math.FourierTransform.DFT(results, FourierTransform.Direction.Forward);

            //// Save FFT results for debugging...
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"fft_result.csv", false))  // do not append
            //{
            //    for (int i = 0; i < waves_complex_part.Count; i++)
            //    {
            //        file.WriteLine("{0},{1}", waves.ElementAt(i).ToString(), results[i].Re);
            //    }
            //}

            //// Reverse the order of the positive freq. range
            //List<XY_Signal> results_rev = new List<XY_Signal>();
            //for (int i = length_FFT - 1; i >= length_FFT / 2; i--)
            //{
            //    double mag = Math.Sqrt(results[i].Re * results[i].Re + results[i].Im * results[i].Im);
            //    //double freq = length_FFT / 2 / 44100.0 * (double)(length_FFT - i);
            //    double freq = 44100/2* (double)(length_FFT - i)/ (length_FFT / 2);
            //    results_rev.Add(new XY_Signal(freq, mag));
            //}

            //// Save FFT results for debugging...
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"fft_result_f.csv", false))  // do not append
            //{
            //    for (int i = 0; i < results_rev.Count; i++)
            //    {
            //        file.WriteLine("{0},{1},{2},{3}", results[length_FFT - 1 - i].Re, results[length_FFT - 1 - i].Im, results_rev[i].x, results_rev[i].y);
            //    }
            //}

            //int div_length = 20;
            //double tmp_sum = 0;
            //double[] tmp_FFT_value = new double[div_length];
            //double[] tmp_FFT_time = new double[div_length];

            //for (int i = 0; i < div_length; i++)
            //{
            //    mAdapter.Begin();

            //    for (int j = i * results_rev.Count / div_length; j < (i + 1) * results_rev.Count / div_length; j++)
            //    {
            //        tmp_sum += results_rev[j].y;
            //    }
            //    tmp_FFT_time[i] = results_rev[i * results_rev.Count / div_length].x;
            //    tmp_FFT_value[i] = tmp_sum / div_length;
            //    tmp_sum = 0;

            //}

            //FFT_time.Values = tmp_FFT_time;
            //FFT_ts.Values = tmp_FFT_value;

            //mAdapter.AddDataItem(FFT_time);
            //mAdapter.AddDataItem(FFT_ts);
            //mAdapter.SendChanged();

            // FFT finished...
            Debug.WriteLine("FFT analysis finished");
        }
        private void btnFFT_Click(object sender, EventArgs e)
        {
            //runFFT();
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

        private void textInterval_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                runInterval = Convert.ToDouble(textInterval.Text);
                timer1.Interval = (int) runInterval * 1000;
            }
        }

        private void btnAutoGathering_Click(object sender, EventArgs e)
        {
            //if(isAuto)
            //{
            //    isAuto = false;
            //    // timer off
            //    signal_sum = 0;
            //    Thread.Sleep(101);
            //    timer1.Stop();
            //    btnAutoGathering.Text = "Start Auto";
            //    btnAutoGathering.Update();
            //} 
            //else
            //{
            //    btnAutoGathering.Text = "Stop Auto";
            //    btnAutoGathering.Update();
            //    // timer start
            //    timer1.Start();
                
            //}
            

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            mAdapter.Begin();
            sample_signal_sum.Value = Math.Abs(signal_sum);

            mAdapter.AddDataItem(sample_signal_sum);
            //mAdapter.AddDataItem(FFT_ts);
            mAdapter.SendChanged();
            Debug.WriteLine("[timer1_tick()] signal_sum=" + signal_sum);
            signal_sum = 0;
            
            
            //runRecordWAV();
            //Debug.Write("Recoding+FFT started");
            //txtStatus.AppendText("Recoding+FFT started"); txtStatus.Update();
            //btnStartRecording.PerformClick();





        }

        private void listBoxRecordings_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
