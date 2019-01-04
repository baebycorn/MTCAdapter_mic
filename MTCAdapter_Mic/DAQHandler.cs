// NI DAQ adapter class
//      For accelerometers and spindle signals
// Huitaek Yun

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// user namespace
using NationalInstruments.DAQmx;
using NationalInstruments;
using System.Data;
using System.Diagnostics;

namespace MTC_adapter_daq
{
    class DAQHandler
    {
               
        //private NationalInstruments.DAQmx.Task myTask;
        private NationalInstruments.DAQmx.Task myTask;
        private NationalInstruments.DAQmx.Task runningTask;
        private AnalogMultiChannelReader reader;
        private AnalogWaveform<double>[] data;
        private AsyncCallback analogCallback;

        private DataTable dataTable = null;
        private DataColumn[] dataColumn = null;

        private double sampleRate = 2000;
        private int signalLength = 0;   // msec. will be assinged at the constructor
        private double rangeMinimum = -5; // Convert.ToDouble(---.Value) <-- use this when I get the value from the form
        private double rangeMaximum = 5;
        private double sensitvity= 100.0;
        private double excitation = 0.004;
        private int samplesPerChannel=2000;
        
        
        private AIAccelerometerSensitivityUnits sensitivityUnits = AIAccelerometerSensitivityUnits.MillivoltsPerG;
        private AITerminalConfiguration terminalConfiguration = AITerminalConfiguration.Pseudodifferential; 
        private AIExcitationSource excitationSource = AIExcitationSource.Internal;
        //private AICoupling inputCoupling = AICoupling.AC;     // defined in DAQHandler() manually...        

        //// If you need trigger...
        //private AnalogEdgeStartTriggerSlope triggerSlope = AnalogEdgeStartTriggerSlope.Rising;
        //private String triggerSource = "APFI0";
        //private double triggerLevel = 0.0;
        //private double triggerHysteresis = 0.0;


        // To save datatable continuosly
        private static double time = 0;
        private static int rowIdx = 0;


        public DAQHandler(int s, int secLength)
        {
            
            sampleRate = s;
            
            // Setting up for accelerometers
            try
            {
                // Create a new task for DAQ
                dataTable = new DataTable { TableName = "DAQTest" }; ;
                myTask = new NationalInstruments.DAQmx.Task("sensors");
                
                AIChannel[] aiChannel = new AIChannel[3];

                aiChannel[0] = myTask.AIChannels.CreateAccelerometerChannel("cDAQ1Mod1/ai0", "",
                            terminalConfiguration, rangeMinimum, rangeMaximum,
                            sensitvity, sensitivityUnits, excitationSource,
                            excitation, AIAccelerationUnits.G);
                aiChannel[1] = myTask.AIChannels.CreateAccelerometerChannel("cDAQ1Mod1/ai1", "",
                            terminalConfiguration, rangeMinimum, rangeMaximum,
                            sensitvity, sensitivityUnits, excitationSource,
                            excitation, AIAccelerationUnits.G);
                aiChannel[2] = myTask.AIChannels.CreateAccelerometerChannel("cDAQ1Mod1/ai2", "",
                            terminalConfiguration, rangeMinimum, rangeMaximum,
                            sensitvity, sensitivityUnits, excitationSource,
                            excitation, AIAccelerationUnits.G);                                            
                
                // Setup the input coupling
                for (int i = 0; i < 3; i++)
                {
                    
                    aiChannel[i].Coupling = AICoupling.AC;
                    
                }

                               
                // Configure the timing parameters
                //myTask.Timing.ConfigureSampleClock("", sampleRate,
                    //SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);

                myTask.Timing.ConfigureSampleClock("", sampleRate,
                    SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1000);

                //// Configure the Analog Trigger
                //myTask.Triggers.StartTrigger.ConfigureAnalogEdgeTrigger(triggerSource, triggerSlope, triggerLevel);
                //myTask.Triggers.StartTrigger.AnalogEdge.Hysteresis = triggerHysteresis;

                // Verify the Task
                myTask.Control(TaskAction.Verify);

                //Prepare the table for Data
                InitializeDataTable(myTask.AIChannels, ref dataTable, secLength);

                signalLength = secLength * Convert.ToInt32(sampleRate);

                runningTask = myTask;
                reader = new AnalogMultiChannelReader(myTask.Stream);
                analogCallback = new AsyncCallback(AnalogInCallback);

                reader.SynchronizeCallbacks = true;
                reader.BeginReadWaveform(samplesPerChannel, analogCallback, myTask);

                

            }        

            catch (DaqException exception)
            {
                // Display Errors
                Console.WriteLine(exception.Message);
                runningTask = null;
                myTask.Dispose();                
            }
        }

        public void measureDAQ()
        {
            try
            {
                Console.WriteLine("measureDAQ() called..");
                data = reader.ReadWaveform(samplesPerChannel);
                dataToDataTable(data, ref dataTable);
            }
            catch (DaqException exception)
            {
                // Display Errors
                Console.WriteLine(exception.Message);
                runningTask = null;
                myTask.Dispose();
            }
            
        }
        public void saveDataToExcel(string fileName)
        {

            dataTable.WriteXml(fileName);            
            
            Debug.WriteLine("DAQ is recorded.");
        }

        public void stopDAQ()
        {
            // Dispose of the task
            runningTask = null;
            myTask.Dispose();
            time = 0;
            rowIdx = 0;

        }

        private void AnalogInCallback(IAsyncResult ar)
        {
            try
            {
                if (runningTask != null && runningTask == ar.AsyncState)
                {
                    // Read the available data from the channels
                    data = reader.EndReadWaveform(ar);

                    // Plot your data here
                    dataToDataTable(data, ref dataTable);
                    Debug.WriteLine("AnalogInCallback");
                    //// Check for and report any overloaded channels
                    //if (overloadDetectionCheckBox.Checked)
                      //  ReportOverloadedChannels();

                    reader.BeginMemoryOptimizedReadWaveform(samplesPerChannel,
                        analogCallback, myTask, data);
                }
            }
            catch (DaqException exception)
            {
                // Display Errors
                Console.WriteLine(exception.Message);
                runningTask = null;
                myTask.Dispose();                
            }
        }

        public void DataTableToBuffer(double[,] buffer)
        {
            int currentLineIndex = 0;

            foreach (AnalogWaveform<double> waveform in data)
            {
                for (int sample = 0; sample < waveform.Samples.Count; ++sample)
                {
                    if (sample == samplesPerChannel)
                        break;

                    buffer[currentLineIndex,sample] = waveform.Samples[sample].Value;
                }
                currentLineIndex++;
            }


        }

        private void dataToDataTable2(AnalogWaveform<double>[] sourceArray, ref DataTable dataTable)
        {
            // Iterate over channels
            int currentLineIndex = 0;
            foreach (AnalogWaveform<double> waveform in sourceArray)
            {
                for (int sample = 0; sample < waveform.Samples.Count; ++sample)
                {
                    if (sample == 10)
                        break;

                    dataTable.Rows[sample][currentLineIndex] = waveform.Samples[sample].Value;
                }
                currentLineIndex++;
            }
        }


        
        private void dataToDataTable(AnalogWaveform<double>[] sourceArray, ref DataTable dataTable)
        {
            
            if(rowIdx < signalLength)
            {
                // Iterate over channels
                int currentLineIndex = 0;
                double increment = 1 / sampleRate;  // sec
                for (int sample = 0; sample < samplesPerChannel; ++sample)
                {
                    dataTable.Rows[rowIdx + sample][currentLineIndex] = time;
                    time = time + increment;


                }
                currentLineIndex++;


                foreach (AnalogWaveform<double> waveform in sourceArray)
                {

                    for (int sample = 0; sample < waveform.Samples.Count; ++sample)
                    {
                        if (sample == samplesPerChannel)
                            break;

                        dataTable.Rows[rowIdx + sample][currentLineIndex] = waveform.Samples[sample].Value;

                    }
                    currentLineIndex++;
                }

                rowIdx += sourceArray[0].Samples.Count;
                Debug.WriteLine("rowIdx=" + rowIdx);
            }
            else
            {
                Debug.WriteLine("Overflow at dataToDataTable()");
            }
            
            
        }

        private void InitializeDataTable(AIChannelCollection channelCollection, ref DataTable data, int secSize)
        {
            int numOfChannels = channelCollection.Count;    // plus time axis
            data.Rows.Clear();
            data.Columns.Clear();
            dataColumn = new DataColumn[numOfChannels+1];
            int numOfRows = samplesPerChannel* secSize;

            dataColumn[0] = new DataColumn();
            dataColumn[0].DataType = typeof(double);
            dataColumn[0].ColumnName = "Time(sec)";

            for (int currentChannelIndex = 0; currentChannelIndex < numOfChannels; currentChannelIndex++)
            {
                dataColumn[currentChannelIndex+1] = new DataColumn();
                dataColumn[currentChannelIndex+1].DataType = typeof(double);
                dataColumn[currentChannelIndex+1].ColumnName = channelCollection[currentChannelIndex].PhysicalName;
            }

            data.Columns.AddRange(dataColumn);

            for (int currentDataIndex = 0; currentDataIndex < numOfRows+1; currentDataIndex++)
            {
                object[] rowArr = new object[numOfChannels+1];
                data.Rows.Add(rowArr);
            }
        }

        public void closeChannel()
        {
            myTask.Dispose();   // stop DAQ task
        }

        public void showChannels()
        {
            string[] chList = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
            foreach (string c in chList)
            {
                Console.WriteLine(c);
            }
        }
    }
}
