using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace XMLGathering
{
    public struct MTCValue
    {
        public int idx;
        public string name; // name in xml
        public string tStamp;
        public double value;

        public string printString()
        {
            return Convert.ToString(idx) + "," + name + "," + tStamp + "," + Convert.ToString(toSec()) + "," + Convert.ToString(value);
        }

        public double toSec()
        {
            double hour, minute, second;
            try
            {
                hour = Convert.ToDouble(tStamp.Substring(11, 2));
                minute = Convert.ToDouble(tStamp.Substring(14, 2));
                second = Convert.ToDouble(tStamp.Substring(17, 6));
            }
            catch (System.NullReferenceException e)
            {
                Console.WriteLine("NullReferenceException source: {0}", e.Source);
                throw;
            }
            return hour * 3600 + minute * 60 + second;
        }
    }
    class MTCXMLParser
    {

        /// <summary>  
        ///  Base class for MTC
        /// </summary>
        

        public int nextSeq;
        public int firstSeq;
        public int lastSeq;

        public MTCXMLParser()
        {

        }

        public void getPrevSequence(string base_url, double secPast, out int seq_prev, out int seq_current)
        {

            int seq;
            string timeStamp_tmp;
            double time=0;
            double time_old=0;

            string url_new = base_url;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;


            // Read MTCOnnect/current to get current seq

            
            using (XmlReader xmlReader = XmlReader.Create(base_url+"/current", settings))
            {

                
                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Header"));

//                timeStamp_tmp = xmlReader.GetAttribute("timestamp");
                seq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));
            }

            seq_current = seq;

            //const int amount = 5000;

            //seq -= amount;
            // for each 100 sec down, check 
            bool firstCall = true;            
            do
            {

                //url_new = base_url + "/sample?from=" + seq + "&count=1";
                url_new = base_url + "/current?at=" + seq;
                //Console.WriteLine(url_new);
                
                using (XmlReader xmlReader = XmlReader.Create(url_new, settings))
                {

                    if(this.GetType() == typeof(MTCXMLParserKukaAudio))
                        xmlReader.ReadToFollowing("SoundLevel");

                    else
                    {
                        do
                        {
                            xmlReader.Read();
                        } while (!xmlReader.Name.Equals("Angle") && !xmlReader.Name.Equals("Position")
                           && !xmlReader.Name.Equals("DisplacementTimeSeries") && !xmlReader.Name.Equals("FrequencyTimeSeries"));
                    }
                

                    if(xmlReader.GetAttribute("timestamp") == null)
                    {
                        seq -= 20;                        
                        //Console.WriteLine("null sensed..");
                        continue;
                    }
                    

                    timeStamp_tmp = xmlReader.GetAttribute("timestamp");
                    
                        
                }

                // Get time as seconds
                double hour, minute, second;

                hour = Convert.ToDouble(timeStamp_tmp.Substring(11, 2));
                minute = Convert.ToDouble(timeStamp_tmp.Substring(14, 2));
                second = Convert.ToDouble(timeStamp_tmp.Substring(17, 6));
                
                time = hour * 3600 + minute * 60 + second;
                if (firstCall)
                {
                    time_old = time;
                    firstCall = false;
                }
                //Console.WriteLine("[debug] seq={0} time_old={1} time={2} diff={3}", seq,time_old, time, time_old-time);
                seq -= 10;

            } while ((time_old-time) < secPast);

            seq_prev = seq+10;
        }




        public void readCurrentSeq(string url)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;


            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {
                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Header"));

                nextSeq = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                lastSeq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));
            }

            //Console.WriteLine("{0} readCurrent() new :firstSeq={1} lastSeq={2} nextSeq={3}", this.ToString(), firstSeq, lastSeq, nextSeq);

        }

        public MTCValue dequeueSafe(Queue<MTCValue> q)
        {
            MTCValue tmpMTCValue;
            tmpMTCValue.idx = 0;
            tmpMTCValue.name = "";
            tmpMTCValue.tStamp = "";

            tmpMTCValue.value = 0.0;

            if (q.Count > 0)
            {
                tmpMTCValue = q.Dequeue();
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"check.txt", true))  // append
                //{
                //    file.WriteLine(tmpMTCValue.printString());
                //}
            }

            while (q.Count > 0 && tmpMTCValue.idx == q.First<MTCValue>().idx)
            {
                tmpMTCValue = q.Dequeue();
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"check.txt", true))  // append
                //{
                //    file.WriteLine("    " + tmpMTCValue.printString());
                //}
            }
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"check.txt", true))  // append
            //{
            //    file.WriteLine("-----------");
            //}

            return tmpMTCValue;
        }


    }

    class MTCXMLParserShark : MTCXMLParser
    {
        public struct DataShark
        {
            public double posX;
            public double posY;
            public double posZ;
        }

        public bool isConnected = false;
        public DataShark collision_init;


        public Queue<MTCValue> queue_posX;
        public Queue<MTCValue> queue_posY;
        public Queue<MTCValue> queue_posZ;

        public const int amount = 96;
        public const int maxQueueSize = 3000;

        public MTCXMLParserShark()
        {
            queue_posX = new Queue<MTCValue>();
            queue_posY = new Queue<MTCValue>();
            queue_posZ = new Queue<MTCValue>();
        }

        public bool isAllQueuesEmpty()
        {
            if (queue_posX.Count <= 0 && queue_posY.Count <= 0 && queue_posZ.Count <= 0)
                return true;
            else
                return false;
        }

        public void printCurrentQueueSize()
        {
            Console.WriteLine("Queue size:x={0} y={1} z={2}",
                queue_posX.Count(), queue_posY.Count(),queue_posZ.Count());
        }

        public DataShark readSharkAllFromCurrent(string url)
        {
            DataShark k;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {
                xmlReader.ReadToFollowing("Availability");
                xmlReader.Read();
                if (xmlReader.Value == "AVAILABLE")
                    isConnected = true;
                else
                    isConnected = false;
            }

            if(isConnected)
            {
                using (XmlReader xmlReader = XmlReader.Create(url, settings))
                {

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Header"));

                    nextSeq = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                    firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                    lastSeq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Position"));
                    xmlReader.Read();

                    if (xmlReader.Value == "UNAVAILABLE")
                    {
                        Console.WriteLine("UNAVAILABLE is detected in MTCOnnect... Quit.");
                        Environment.Exit(1);
                    }


                    k.posX = Convert.ToDouble(xmlReader.Value);


                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Position"));

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Position"));
                    xmlReader.Read();
                    k.posY = Convert.ToDouble(xmlReader.Value);
                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Position"));

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Position"));
                    xmlReader.Read();
                    k.posZ = Convert.ToDouble(xmlReader.Value);
                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Position"));

                    xmlReader.ReadToFollowing("Availability");
                    xmlReader.Read();
                    if (xmlReader.Value == "Available")
                        isConnected = false;
                    else
                        isConnected = true;
                }
            }
            else
            {
                k.posX = 0;
                k.posY = 0;
                k.posZ = 0;
            }
            
            return k;
        }

        // Read all data from "current" page
        public void readSharkFromSample(string url, object lck)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {

                //xmlReader.MoveToContent();

                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Position"));

                //Console.WriteLine("readKukaCurrent() new :firstSeq={0} lastSeq={1} nextSeq={2}", firstSeq, lastSeq, nextSeq);


                while(true)
                {
                    //do
                    //{
                    //    xmlReader.Read();
                    //    //Console.WriteLine("{0} read1", xmlReader.Name);
                    //} while (!xmlReader.Name.Equals("Position") && !xmlReader.Name.Equals("MTConnectStreams"));

                    //if (xmlReader.Name.Equals("MTConnectStreams"))
                    //    break;                    
                    
                    MTCValue tmpMTCValue;
                    tmpMTCValue.idx = Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                    tmpMTCValue.name = xmlReader.GetAttribute("name");
                    tmpMTCValue.tStamp = xmlReader.GetAttribute("timestamp");



                    //// Maintain maximum queue size by dequeueing                        
                    //if (queue_posX.Count > maxQueueSize)
                    //{
                    //    lock (lck)
                    //    {
                    //    queue_posX.Dequeue();

                    //    }
                    //}

                    //// Maintain maximum queue size by dequeueing                        
                    //if (queue_posY.Count > maxQueueSize)
                    //{
                    //    lock (lck)
                    //    {
                    //    queue_posY.Dequeue();

                    //    }
                    //}

                    //// Maintain maximum queue size by dequeueing                        
                    //if (queue_posZ.Count > maxQueueSize)
                    //{
                    //    lock (lck)
                    //    {
                    //    queue_posZ.Dequeue();

                    //    }
                    //}



                    Queue<MTCValue> queueRef = null;
                    
                    string s = xmlReader.GetAttribute("name");
                    switch (s)
                    {
                        case "xPosition":
                            queueRef = queue_posX;
                            break;
                        case "yPosition":
                            queueRef = queue_posY;
                            break;
                        case "zPosition":
                            queueRef = queue_posZ;
                            break;                           
                    }


                    xmlReader.Read();   // read value
                    tmpMTCValue.value = Convert.ToDouble(xmlReader.Value);
                    

                    //lock (lck)
                    //{
                        queueRef.Enqueue(tmpMTCValue);
                    //}

                    Console.WriteLine("Value:{0}", xmlReader.Value);
                    xmlReader.Read();   // read </position>

                    do
                    {
                        xmlReader.Read();
                        Console.WriteLine("{0} read1", xmlReader.Name);
                    } while (!(xmlReader.Name.Equals("Position") || xmlReader.Name.Equals("MTConnectStreams")));

                        if (xmlReader.Name.Equals("MTConnectStreams"))
                            break;     

                    }


            }

        }


        public void readSharkFromSample2(string url, object lck, int seq_current)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            //settings.ConformanceLevel = ConformanceLevel.Fragment;

            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {
                
                Queue<MTCValue> queueRef = null;
                MTCValue tmpMTCValue = new MTCValue();
                
                while (true)
                {

                    if(!xmlReader.ReadToFollowing("Position"))
                        break;
                    

                    if(xmlReader.IsStartElement())
                    {
                        
                        tmpMTCValue.idx = Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                        tmpMTCValue.name = xmlReader.GetAttribute("name");
                        tmpMTCValue.tStamp = xmlReader.GetAttribute("timestamp");                      
                            

                        string s = xmlReader.GetAttribute("name");
                        switch (s)
                        {
                            case "xPosition":
                                queueRef = queue_posX;
                                break;
                            case "yPosition":
                                queueRef = queue_posY;
                                break;
                            case "zPosition":
                                queueRef = queue_posZ;
                                break;
                        }
                        xmlReader.Read();   // read value                        
                        tmpMTCValue.value = Convert.ToDouble(xmlReader.Value);

                        if (tmpMTCValue.idx <= seq_current)
                        {
                            //Console.WriteLine("Value:{0}", xmlReader.Value);
                            queueRef.Enqueue(tmpMTCValue);
                        }
                        else
                        {
                            //Console.WriteLine("Bypass..");
                        }

                    }                  
                    
                    
                    

                    

                }


            }

        }

    }



    class MTCXMLParserKuka_forAudio : MTCXMLParser
    {
        public List<MTCValue>[] list_angles = new List<MTCValue>[6];
        public List<MTCValue>[] list_torques = new List<MTCValue>[6];

        public MTCValue[] current_angles = new MTCValue[6];


        
        public MTCXMLParserKuka_forAudio()
        {
            for (int i = 0; i < 6; i++)
            {
                list_angles[i] = new List<MTCValue>();
                list_torques[i] = new List<MTCValue>();
            }
        }

        public void readPositionFromCurrent(string url)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {

                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Header"));

                nextSeq = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                lastSeq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));

                int idx = -1;
                while (!xmlReader.EOF)
                {

                    xmlReader.Read();
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.GetAttribute("dataItemId"))
                        {
                            case "A_1": idx = 0; break;
                            case "A_2": idx = 1; break;
                            case "A_3": idx = 2; break;
                            case "A_4": idx = 3; break;
                            case "A_5": idx = 4; break;
                            case "A_6": idx = 5; break;
                            default: idx=-1; break;
                        }


                        if (idx != -1)
                        {
                            

                            current_angles[idx].idx= Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                            current_angles[idx].tStamp = xmlReader.GetAttribute("timestamp");
                            current_angles[idx].name = xmlReader.GetAttribute("name");

                            xmlReader.Read();

                            if (xmlReader.Value == "UNAVAILABLE")
                            {
                                Debug.WriteLine("UNAVAILABLE is detected.");
                                System.Windows.Forms.Application.Exit();
                            }
                            else
                            {
                                current_angles[idx].value = Convert.ToDouble(xmlReader.Value);                                
                            }
                        }
                    }



                }



            }
        }

        public void readFromSample(string url)
        {

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {

                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Header"));

                nextSeq = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                lastSeq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));


                List<MTCValue> targetList = null;
                while(!xmlReader.EOF)
                {
                    
                    xmlReader.Read();
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.GetAttribute("dataItemId"))
                        {
                            case "A_1": targetList = list_angles[0]; break;
                            case "A_2": targetList = list_angles[1]; break;
                            case "A_3": targetList = list_angles[2]; break;
                            case "A_4": targetList = list_angles[3]; break;
                            case "A_5": targetList = list_angles[4]; break;
                            case "A_6": targetList = list_angles[5]; break;

                            case "A_1_torque": targetList = list_torques[0]; break;
                            case "A_2_torque": targetList = list_torques[1]; break;
                            case "A_3_torque": targetList = list_torques[2]; break;
                            case "A_4_torque": targetList = list_torques[3]; break;
                            case "A_5_torque": targetList = list_torques[4]; break;
                            case "A_6_torque": targetList = list_torques[5]; break;

                            default: targetList = null; break;
                        }


                        if (targetList != null)
                        {
                            MTCValue tmpMTCValue;

                            tmpMTCValue.idx = Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                            tmpMTCValue.tStamp = xmlReader.GetAttribute("timestamp");
                            tmpMTCValue.name = xmlReader.GetAttribute("name");

                            xmlReader.Read();

                            if (xmlReader.Value == "UNAVAILABLE")
                            {
                                Debug.WriteLine("UNAVAILABLE is detected.");
                                System.Windows.Forms.Application.Exit();
                            }
                            else
                            {
                                tmpMTCValue.value = Convert.ToDouble(xmlReader.Value);
                                targetList.Add(tmpMTCValue);
                            }
                        }
                    }

                    

                }               

            }

            // check if there is no position. It means taht the robot angle is stopped.
            // So put a value to the list.
            // We don't need this job in torque. It varies all the time.

            Debug.Write("Pos=");
            for (int i=0; i<6; i++)
            {
                
                Debug.Write(list_angles[i].Count + ",");
                if (list_angles[i].Count == 0)
                {
                    list_angles[i].Add(current_angles[i]);
                }
            }
            Debug.Write("\n");


            Debug.Write("Torque=");
            for (int i = 0; i < 6; i++)
            {

                Debug.Write(list_torques[i].Count + ",");                
            }
            Debug.Write("\n");
        }






    }

    class MTCXMLParserKuka : MTCXMLParser
    {

        public struct DataKuka
        {
            public int feedrate;
            public string mode;
            public double a1;
            public double a2;
            public double a3;
            public double a4;
            public double a5;
            public double a6;

            public double p1;
            public double p2;
            public double p3;
            public double p4;
            public double p5;
            public double p6;

            public double t1;
            public double t2;
            public double t3;
            public double t4;
            public double t5;
            public double t6;
        }

        public Queue<MTCValue> queue_a1;
        public Queue<MTCValue> queue_a2;
        public Queue<MTCValue> queue_a3;
        public Queue<MTCValue> queue_a4;
        public Queue<MTCValue> queue_a5;
        public Queue<MTCValue> queue_a6;
                
        public const int amount = 96;
        public const int maxQueueSize = 600;

        public DataKuka collision_init;

        public bool isConnected = false;
        
        
        public MTCXMLParserKuka(): base()
        {
            queue_a1 = new Queue<MTCValue>();
            queue_a2 = new Queue<MTCValue>();
            queue_a3 = new Queue<MTCValue>();
            queue_a4 = new Queue<MTCValue>();
            queue_a5 = new Queue<MTCValue>();
            queue_a6 = new Queue<MTCValue>();            
            
        }

        
        public void printCurrentQueueSize()
        {
            Console.WriteLine("Queue size:a1={0} a2={1} a3={2} a4={3} a5={4} a6={5}", 
                queue_a1.Count(), queue_a2.Count(), queue_a3.Count(), queue_a4.Count(), queue_a5.Count(), queue_a6.Count());
        }

        public bool isAllQueuesEmpty()
        {
            if (queue_a1.Count <= 0 && queue_a2.Count <= 0 && queue_a3.Count <= 0 && queue_a4.Count <= 0 
                && queue_a5.Count <= 0 && queue_a6.Count <= 0)
                return true;
            else
                return false;
        }

        public bool isLastSeq()
        {
            if (nextSeq == (lastSeq + 1))
                return true;
            else
                return false;

        }
                

        public void readKukaCurrentSeq(string url)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            
            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {
                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Header"));
                
                nextSeq  = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                lastSeq  = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));
            }
            
            Console.WriteLine("readKukaCurrent() new :firstSeq={0} lastSeq={1} nextSeq={2}", firstSeq, lastSeq, nextSeq);

        }


        public void readKukaCurrent_joints(string url, ref double[] joints)
        {                        
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            
            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {
                for(int i=0; i<6; i++)
                {
                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    joints[i] = Convert.ToDouble(xmlReader.Value);
                }                
            }            
        }

        






        public DataKuka readKukaAllFromCurrent(string url)
        {
            DataKuka k;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;


            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {
                xmlReader.ReadToFollowing("Availability");
                xmlReader.Read();
                if (xmlReader.Value == "AVAILABLE")
                    isConnected = true;
                else
                    isConnected = false;
            }

            if(isConnected)
            {
                using (XmlReader xmlReader = XmlReader.Create(url, settings))
                {

                    //xmlReader.MoveToContent();

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Header"));

                    nextSeq = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                    firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                    lastSeq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("AxisFeedrate"));
                    xmlReader.Read();

                    if (xmlReader.Value == "UNAVAILABLE")
                    {
                        Console.WriteLine("UNAVAILABLE is detected in MTCOnnect... Quit.");
                        Environment.Exit(1);
                    }


                    k.feedrate = Convert.ToInt32(xmlReader.Value);

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("MachineState"));
                    xmlReader.Read();
                    k.mode = xmlReader.Value;


                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.a1 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Torque");
                    xmlReader.Read();
                    k.t1 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.a2 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Torque");
                    xmlReader.Read();
                    k.t2 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.a3 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Torque");
                    xmlReader.Read();
                    k.t3 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.a4 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Torque");
                    xmlReader.Read();
                    k.t4 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.a5 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Torque");
                    xmlReader.Read();
                    k.t5 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.a6 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Torque");
                    xmlReader.Read();
                    k.t6 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.p1 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.p2 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Angle");
                    xmlReader.Read();
                    k.p3 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Position");
                    xmlReader.Read();
                    k.p4 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Position");
                    xmlReader.Read();
                    k.p5 = Convert.ToDouble(xmlReader.Value);

                    xmlReader.ReadToFollowing("Position");
                    xmlReader.Read();
                    k.p6 = Convert.ToDouble(xmlReader.Value);




                }
                
            } else
            {
                k.feedrate = 0;
                k.mode = "";
                k.a1 = k.a2 = k.a3 = k.a4 = k.a5 = k.a6 = 0;
                k.p1 = k.p2 = k.p3 = k.p4 = k.p5 = k.p6 = 0;
                k.t1 = k.t2 = k.t3 = k.t4 = k.t5 = k.t6 = 0;
            }
            

            return k;
        }

        public void readKukaAngleFromSample(string url, object lck, int seq_current)
        {
            

            //Console.WriteLine("address:{0}", url);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {

                //xmlReader.MoveToContent();
                
                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Header"));

                nextSeq = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                lastSeq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));
               // Console.WriteLine("readKukaAngleFromSample() new :firstSeq={0} lastSeq={1} nextSeq={2}", firstSeq, lastSeq, nextSeq);




                //  Temp variables for while loop
                
                while (true)
                {

                    
                    do
                    {
                        xmlReader.Read();
                        
                    } while (!xmlReader.Name.Equals("Angle") && !xmlReader.Name.Equals("MTConnectStreams"));

                    if (xmlReader.Name.Equals("MTConnectStreams")) break;

                    MTCValue tmpMTCValue;
                    string s;
                    Queue<MTCValue> queueRef = null;


                    tmpMTCValue.idx = Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                    tmpMTCValue.tStamp = xmlReader.GetAttribute("timestamp");
                    tmpMTCValue.name= xmlReader.GetAttribute("name");


                    lock (lck)
                    {
                        if (queue_a1.Count > maxQueueSize) queue_a1.Dequeue();
                        if (queue_a2.Count > maxQueueSize) queue_a2.Dequeue();
                        if (queue_a3.Count > maxQueueSize) queue_a3.Dequeue();
                        if (queue_a4.Count > maxQueueSize) queue_a4.Dequeue();
                        if (queue_a5.Count > maxQueueSize) queue_a5.Dequeue();
                        if (queue_a6.Count > maxQueueSize) queue_a6.Dequeue();
                    }

                    s = xmlReader.GetAttribute("name");

                    //Console.WriteLine("Name={0} Sequence={1} string ={2}", xmlReader.Name, tmpMTCValue.idx, s);

                    switch (s)
                    {
                        case "a1":
                            queueRef = queue_a1;
                            break;
                        case "a2":
                            queueRef = queue_a2;
                            break;
                        case "a3":
                            queueRef = queue_a3;
                            break;
                        case "a4":
                            queueRef = queue_a4;
                            break;
                        case "a5":
                            queueRef = queue_a5;
                            break;
                        case "a6":
                            queueRef = queue_a6;
                            break;
                    }
                    
                    xmlReader.Read();   // read value
                    //Console.WriteLine("{0} read", xmlReader.Value);
                    tmpMTCValue.value = Convert.ToDouble(xmlReader.Value);

                    lock (lck)
                    {
                        if (tmpMTCValue.idx <= seq_current)
                            queueRef.Enqueue(tmpMTCValue);
                    }

                    //Console.WriteLine("{0} queue, {1} string, {2} value", queueRef.ToString(), s, tmpMTCValue.value);

                    //Console.WriteLine("Value:{0}", xmlReader.Value);
                    xmlReader.Read();   // read </angle>             

                    //Console.WriteLine("Read:{0}", xmlReader.Name);



                } 
            }

        }

        




    }

    class MTCXMLParserKukaAudio:MTCXMLParser
    {
        // variables
        public double ampl=0;
        public bool isConnected=false;
        public Queue<MTCValue> queue = new Queue<MTCValue>();

        public void readKUKAAudioFromCurrent(string url)
        {
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;


            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {
                xmlReader.ReadToFollowing("Availability");
                xmlReader.Read();
                if (xmlReader.Value == "AVAILABLE")
                    isConnected = true;
                else
                    isConnected = false;
            }

            if (isConnected)
            {
                using (XmlReader xmlReader = XmlReader.Create(url, settings))
                {

                    //xmlReader.MoveToContent();

                    do
                    {
                        xmlReader.Read();
                    } while (!xmlReader.Name.Equals("Header"));

                    nextSeq = Convert.ToInt32(xmlReader.GetAttribute("nextSequence"));
                    firstSeq = Convert.ToInt32(xmlReader.GetAttribute("firstSequence"));
                    lastSeq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));
                                        
                    xmlReader.ReadToFollowing("SoundLevel");

                    MTCValue tmpMTCValue;
                    tmpMTCValue.idx = Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                    tmpMTCValue.name = xmlReader.GetAttribute("name");
                    tmpMTCValue.tStamp = xmlReader.GetAttribute("timestamp");

                    xmlReader.Read();
                    tmpMTCValue.value = Convert.ToDouble(xmlReader.Value);
                    
                }

            }
            else
            {
                
            }
        }


        public void getPrevSequence(string base_url, double secPast, out int seq_prev, out int seq_current)
        {

            int seq;
            string timeStamp_tmp;
            double time = 0;
            double time_old = 0;

            string url_new = base_url;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;


            // Read MTCOnnect/current to get current seq


            using (XmlReader xmlReader = XmlReader.Create(base_url + "/current", settings))
            {


                do
                {
                    xmlReader.Read();
                } while (!xmlReader.Name.Equals("Header"));

                //                timeStamp_tmp = xmlReader.GetAttribute("timestamp");
                seq = Convert.ToInt32(xmlReader.GetAttribute("lastSequence"));
            }

            seq_current = seq;

            //const int amount = 5000;

            //seq -= amount;
            // for each 100 sec down, check 
            bool firstCall = true;
            do
            {

                //url_new = base_url + "/sample?from=" + seq + "&count=1";
                url_new = base_url + "/current?at=" + seq;
                //Console.WriteLine(url_new);

                using (XmlReader xmlReader = XmlReader.Create(url_new, settings))
                {

                    if (this.GetType() == typeof(MTCXMLParserKukaAudio))
                        xmlReader.ReadToFollowing("SoundLevel");

                    else
                    {
                        do
                        {
                            xmlReader.Read();
                        } while (!xmlReader.Name.Equals("Angle") && !xmlReader.Name.Equals("Position")
                           && !xmlReader.Name.Equals("DisplacementTimeSeries") && !xmlReader.Name.Equals("FrequencyTimeSeries"));
                    }


                    if (xmlReader.GetAttribute("timestamp") == null)
                    {
                        seq -= 20;
                        //Console.WriteLine("null sensed..");
                        continue;
                    }


                    timeStamp_tmp = xmlReader.GetAttribute("timestamp");


                }

                // Get time as seconds
                double hour, minute, second;

                hour = Convert.ToDouble(timeStamp_tmp.Substring(11, 2));
                minute = Convert.ToDouble(timeStamp_tmp.Substring(14, 2));
                second = Convert.ToDouble(timeStamp_tmp.Substring(17, 6));

                time = hour * 3600 + minute * 60 + second;
                if (firstCall)
                {
                    time_old = time;
                    firstCall = false;
                }
                //Console.WriteLine("[debug] seq={0} time_old={1} time={2} diff={3}", seq, time_old, time, time_old - time);
                seq -= 10;

            } while ((time_old - time) < secPast);

            seq_prev = seq + 10;
        }


        public void readKukaAudioFromSample(string url)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            //settings.ConformanceLevel = ConformanceLevel.Fragment;

            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {

                MTCValue tmpMTCValue = new MTCValue();

                
                while (true)
                {

                    xmlReader.ReadToFollowing("SoundLevel");

                    if (xmlReader.GetAttribute("timestamp") == null)
                        break;

                    tmpMTCValue.idx = Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                    tmpMTCValue.name = xmlReader.GetAttribute("name");
                    tmpMTCValue.tStamp = xmlReader.GetAttribute("timestamp");
                    xmlReader.Read();   // read value                        

                    if (xmlReader.Value == "UNAVAILABLE")
                        continue;

                    tmpMTCValue.value = Convert.ToDouble(xmlReader.Value);

                    queue.Enqueue(tmpMTCValue);
                }

                
            }
        }

        public void readKukaAudioFromSample2(string url)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            //settings.ConformanceLevel = ConformanceLevel.Fragment;

            bool firstCall = true;

            double time_old = 0;
            double time_new = 0;
            using (XmlReader xmlReader = XmlReader.Create(url, settings))
            {

                MTCValue tmpMTCValue = new MTCValue();


                while (true)
                {

                    xmlReader.ReadToFollowing("SoundLevel");

                    if (xmlReader.GetAttribute("timestamp") == null)
                        break;

                    tmpMTCValue.idx = Convert.ToInt32(xmlReader.GetAttribute("sequence"));
                    tmpMTCValue.name = xmlReader.GetAttribute("name");
                    tmpMTCValue.tStamp = xmlReader.GetAttribute("timestamp");
                    xmlReader.Read();   // read value                        

                    if (xmlReader.Value == "UNAVAILABLE")
                        continue;

                    tmpMTCValue.value = Convert.ToDouble(xmlReader.Value);

                    queue.Enqueue(tmpMTCValue);

                    time_new = tmpMTCValue.toSec();

                    if (firstCall)
                    {
                        time_old = time_new;
                        firstCall = false;
                    }
                    //Console.WriteLine("time_old={0} time_new={1} time_diff={2}", time_old, time_new, time_new - time_old);
                    if (time_new-time_old> 10)
                        break;                    
                }


            }
        }
    }
}
