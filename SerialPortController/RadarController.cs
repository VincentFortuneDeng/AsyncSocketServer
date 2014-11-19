using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SenserModels;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Diagnostics;
using SenserModels.Config;
using SenserModels.Entity;
using System.Windows.Forms;

namespace SerialPortController
{
    public class RadarController : SenserController
    {
        /// <summary>
        /// 记录队列
        /// </summary>
        private Dictionary<RtuRecordKey, byte[]> recordQueueData;
        private BaseSettings initSettings;


        private string nodeID;

        public string NodeID
        {
            get { return nodeID; }
            set { nodeID = value; }
        }



        /// <summary>
        /// 距离传感器列表
        /// </summary>
        //private new System.Collections.Generic.Dictionary<int, SerialPortController.DataCollector> senserDictionary;
        /// <summary>
        /// 数据处理器
        /// </summary>
        private DataCenter dataCenter;
        /// <summary>
        /// 数据键队列
        /// </summary>
        private Queue<RtuRecordKey> recordQueueKey;
        /// <summary>
        /// 效率分析器
        /// </summary>
        private Analyzer analyzer;

        public Analyzer Analyzer
        {
            get { return analyzer; }
            set { analyzer = value; }
        }
        /// <summary>
        /// 轮询模式
        /// </summary>
        private PollModeType pollMode;

        protected override void InitMembers()
        {
            this.analyzer = new Analyzer();

            this.dataCenter = new DataCenter(Configs<BaseSettings>.GetConfig().DeviceRange[DeviceType.ELEP]);

            this.pollMode = PollModeType.None;

            this.recordQueueData = new Dictionary<RtuRecordKey, byte[]>();

            this.LoadSetting();

            //this.recordQueueData.Add(DeviceType.EP, new Dictionary<string, byte[]>());

            //this.recordQueueData.Add(DeviceType.PIP, new Dictionary<string, byte[]>());

            //this.recordQueueData.Add(DeviceType.POP, new Dictionary<string, byte[]>());

            //this.recordQueueData.Add(DeviceType.UF, new Dictionary<string, byte[]>());

            //this.recordQueueData.Add(DeviceType.WDSP, new Dictionary<string, byte[]>());

            //this.recordQueueData.Add(DeviceType.WISP, new Dictionary<string, byte[]>());

            //this.recordQueueData.Add(DeviceType.WIWP, new Dictionary<string, byte[]>());

            this.recordQueueKey = new Queue<RtuRecordKey>();
        }



        public RadarController(ReportWorkMode workMode, params int[] pollInterval)
            : base(workMode, pollInterval)
        {
            //this.IinitSensers();
        }

        public void Reset()
        {
            //this.analyzer = new Analyzer();

            this.currentSenser = null;

            //this.dataCenter = new DataCenter();

            this.indexSenser = 0;

            this.recordQueueData.Clear();

            //this.recordQueueData[DeviceType.PIP].Clear();

            //this.recordQueueData[DeviceType.POP].Clear();

            //this.recordQueueData[DeviceType.UF].Clear();

            //this.recordQueueData[DeviceType.WDSP].Clear();

            //this.recordQueueData[DeviceType.WISP].Clear();

            //this.recordQueueData[DeviceType.WIWP].Clear();




            //this.pollInterval = pollInterval;

            //this.pollMode = PollModeType.None;

            //this.pollSignal = new object();

            //this.recordQueueData = new Dictionary<DeviceType, Queue<byte[]>>();

            //this.recordQueueData.Add(DeviceType.EP, new Queue<byte[]>());

            //this.recordQueueData.Add(DeviceType.PIP, new Queue<byte[]>());

            //this.recordQueueData.Add(DeviceType.POP, new Queue<byte[]>());

            //this.recordQueueData.Add(DeviceType.UF, new Queue<byte[]>());

            //this.recordQueueData.Add(DeviceType.Unknown, new Queue<byte[]>());

            //this.recordQueueData.Add(DeviceType.WDSP, new Queue<byte[]>());

            //this.recordQueueData.Add(DeviceType.WISP, new Queue<byte[]>());

            //this.recordQueueData.Add(DeviceType.WIWP, new Queue<byte[]>());

            //this.reportSignal = new object();

            //this.recordQueueKey = new Dictionary<string, int>();
            this.recordQueueKey.Clear();

        }

        public PollModeType PollMode
        {
            get { return pollMode; }
            set { pollMode = value; }
        }

        /// <summary>
        /// 添加通讯控制器
        /// </summary>
        /// <param name="setting">设置</param>
        protected override SerialListener AddComController(string setting)
        {
            string[] comSettings = setting.Split(',');
            CollecterListener collecterComController = new CollecterListener(comSettings[0],
                int.Parse(comSettings[1]), this.workMode, false);

            collecterComController.ComOnEvent += new EventHandler<ComOnEventArgs>(ComController_ComOnEvent);

            this.m_listenerDictionary.Add(comSettings[0], collecterComController);
            return collecterComController;
        }

        protected override void ComController_ReportEvent(object sender, EventArgs e)
        {

        }

        protected override void ComController_ComOnEvent(object sender, ComOnEventArgs e)
        {
            this.pollMode = PollModeType.None;
            base.ComController_ComOnEvent(sender, e);
        }

        /// <summary>
        /// 创建传感器列表
        /// </summary>
        protected override void AddSenser(SerialListener collecterComController, string addressList)
        {
            this.pollRun = false;
            string[] addresses = addressList.Split(',');
            foreach (string address in addresses)
            {
                byte byteAddress = Convert.ToByte(address, 10);
                DataCollector dataCollector = new DataCollector(byteAddress, (CollecterListener)collecterComController);
                dataCollector.WorkStateEvent += new EventHandler<WorkStateEventArgs>(Senser_WorkStateEvent);

                this.senserDictionary.Add(byteAddress, dataCollector);

                //this.recordDataQueue.Add(byteAddress, new Dictionary<string, DistanceData>());
            }
            this.pollRun = true;
            //for (byte i = 1; i <= maxAddress; i++)
            //{
            //    DataCollector dataCollector = new DataCollector(i, (CollecterComController)collecterComController);
            //    dataCollector.WorkStateEvent += new EventHandler<WorkStateEventArgs>(Senser_WorkStateEvent);

            //    this.senserDictionary.Add(i, dataCollector);
            //}
        }



        /// <summary>
        /// 输出线程
        /// </summary>
        protected override void ReportEventHandler()
        {
            try
            {
                RtuRecordKey recordKey = null;
                //线程循环
                while (true)
                {
                    //等待接收线程的报告通知
                    while (null == recordKey)
                    {
                        lock (reportSignal)
                        {
                            //如果报告队列中已经存在报告 则取出报告包进行处理
                            if (0 != recordQueueKey.Count)
                            {
                                recordKey = recordQueueKey.Dequeue();
                            }

                            //如果没有报告则等待报告(线程等待)
                            else
                            {
                                this.dataCenter.UpdateCollectingData();

                                Monitor.Wait(reportSignal);
                            }
                        }
                    }//end 等待接收线程的报告通知

                    this.dataCenter.InsertRecord(recordKey, this.recordQueueData[recordKey]);
                    /*
                    if (!this.dataCenter.InsertRecord(recordKey, this.recordQueueData[recordKey]))
                    {
                        MessageBox.Show(recordKey.DataDateTime + " " + recordKey.DeviceType + " 数据异常");
                    }*/
                    //this.dataCenter.UpdateCollectingData();

                    ////处理报告包
                    //switch (packet.Type)
                    //{

                    //}
                    recordKey = null;
                }
            }

            catch (IOException ioe)
            {
                // abort the thread
                //System.Threading.Thread.CurrentThread.Abort();
                throw ioe;
            }

            catch (ObjectDisposedException ode)
            {
                if (reportThread != null)
                {
                    reportThread = null;
                }
                throw ode;
            }
        }

        private void PollAll()
        {
            pollStoped = false;
            byte[] data = new byte[] { };
            this.currentSenser = (DataCollector)this.NextSenser();

            switch (this.currentSenser.WorkState)
            {
                case DeviceWorkState.OnWork:
                    data = ((DataCollector)this.currentSenser).ReadAllRecords();

                    if (data != null)
                    {
                        SplitRecordData(((DataCollector)this.currentSenser).DeviceType, data, ((DataCollector)this.currentSenser).SingleRecordLength);
                    }
                    break;

                case DeviceWorkState.OnReady:
                    ((DataCollector)this.currentSenser).RestartSampling();

                    break;

                case DeviceWorkState.Fault:
                    if (((DataCollector)this.currentSenser).Ping())
                    {
                        if (((DataCollector)this.currentSenser).DeviceType == DeviceType.Unknown)
                        {
                            ((DataCollector)this.currentSenser).InitSenser(this.initSettings.DeviceRange);
                        }
                    }


                    break;

                default:

                    break;
            }
        }

        /// <summary>
        /// 获取数据线程
        /// </summary>
        protected override void PollHandler()
        {
            try
            {
                //线程循环
                while (true)
                {
                    if (this.pollRun)
                    {
                        switch (this.pollMode)
                        {
                            case PollModeType.None:
                                PollStop();
                                break;

                            case PollModeType.REAL_TIME:/*
                            AsyncMethodCallerReturnData readRealTimeRecordCaller =
                                new AsyncMethodCallerReturnData(this.currentSenser.ReadRealTimeRecord);
                            readRealTimeRecordCaller.BeginInvoke(new AsyncCallback(CallbackMethodReturnData), null);
                            */

                                PollRealTime();


                                break;

                            case PollModeType.LAST_RECORD:
                                /*AsyncMethodCallerReturnData readLastRecordCaller =
                                    new AsyncMethodCallerReturnData(this.currentSenser.ReadLastRecord);
                                readLastRecordCaller.BeginInvoke(new AsyncCallback(CallbackMethodReturnData), null);*/

                                PollLastRecord();

                                break;

                            case PollModeType.ALL:
                                /*AsyncMethodCallerReturnData readAllRecordsCaller =
                                    new AsyncMethodCallerReturnData(this.currentSenser.ReadAllRecords);
                                readAllRecordsCaller.BeginInvoke(new AsyncCallback(CallbackMethodReturnData), null);*/

                                PollAll();

                                break;

                            default:

                                break;
                        }


                        Thread.Sleep(this.PollInterval);

                        //Thread.Sleep(500);
                    }

                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
            }

            catch (IOException ioe)
            {
                // abort the thread
                //System.Threading.Thread.CurrentThread.Abort();

                throw ioe;
            }

            catch (ObjectDisposedException ode)
            {
                if (pollThread != null)
                {
                    pollThread = null;
                }
                throw ode;
            }

        }

        private bool pollStoped;

        public bool PollStoped
        {
            get { return pollStoped; }
            //set { pollStoped = value; }
        }

        private void PollStop()
        {
            foreach (DataCollector collector in this.senserDictionary.Values)
            {
                if (collector.WorkState == DeviceWorkState.OnWork)
                {
                    collector.StopSampling();
                }
            }
            pollStoped = true;
        }

        private void PollLastRecord()
        {
            pollStoped = false;
            byte[] data = new byte[] { };
            this.currentSenser = (DataCollector)this.NextSenser();
            switch (this.currentSenser.WorkState)
            {
                case DeviceWorkState.OnWork:
                    data = ((DataCollector)this.currentSenser).ReadLastRecord();

                    if (data != null)
                    {
                        SplitRecordData(((DataCollector)this.currentSenser).DeviceType, data, ((DataCollector)this.currentSenser).SingleRecordLength);
                    }
                    break;

                case DeviceWorkState.OnReady:
                    ((DataCollector)this.currentSenser).RestartSampling();

                    break;

                case DeviceWorkState.Fault:
                    if (((DataCollector)this.currentSenser).Ping())
                    {
                        if (((DataCollector)this.currentSenser).DeviceType == DeviceType.Unknown)
                        {
                            ((DataCollector)this.currentSenser).InitSenser(this.initSettings.DeviceRange);
                        }
                    }

                    break;

                default:

                    break;
            }
        }

        private void PollRealTime()
        {
            pollStoped = false;
            byte[] data = new byte[] { };
            this.currentSenser = (DataCollector)this.NextSenser();
            if (this.currentSenser != null)
            {
                switch (this.currentSenser.WorkState)
                {
                    case DeviceWorkState.OnWork:
                        data = ((DataCollector)this.currentSenser).ReadRealTimeRecord();

                        if (data != null)
                        {
                            SplitRecordData(((DataCollector)this.currentSenser).DeviceType, data, ((DataCollector)this.currentSenser).SingleRecordLength);
                        }
                        break;

                    case DeviceWorkState.OnReady:
                        ((DataCollector)this.currentSenser).RestartSampling();

                        break;

                    case DeviceWorkState.Fault:
                        if (((DataCollector)this.currentSenser).Ping())
                        {
                            if (((DataCollector)this.currentSenser).DeviceType == DeviceType.Unknown)
                            {
                                ((DataCollector)this.currentSenser).InitSenser(this.initSettings.DeviceRange);
                            }
                        }

                        break;

                    default:

                        break;
                }
            }
        }

        protected override void LoadSetting()
        {
            this.baseSettings = SerialPortController.Properties.Settings.Default.BaseSetting;
            this.initSettings = Configs<BaseSettings>.GetConfig();
            //this.nodeID = this.initSettings.MotorID;
        }

        protected override ISenser NextSenser()
        {
            int i = 0;
            DataCollector outDataCollector = null;
            foreach (DataCollector dataCollector in this.senserDictionary.Values)
            {
                if (indexSenser == i++ && dataCollector.ComOn)
                {
                    outDataCollector = dataCollector;
                    break;
                }
            }
            indexSenser = ++indexSenser % this.senserDictionary.Count;

            return outDataCollector;
        }

        /// <summary>
        /// 拆解数据
        /// </summary>
        /// <param name="data">要拆分的数据 返回包含时间的数据</param>
        protected override void SplitRecordData(DeviceType deviceType, byte[] data, int singleRecordLength)
        {
            string date = DateTime.Now.Date.ToString("yyyy-MM-dd ");
            string dateTime = string.Empty;
            //using (StreamWriter sw = new StreamWriter(@".\Debug.log", true))
            //{
            /*sw.WriteLine(deviceType.ToString() + " singleRecordLength:" + singleRecordLength);*/
            for (int index = 0; index < data.Length; index += singleRecordLength)
            {
                UInt32 time = (UInt32)(data[index + 0] << 24 | data[index + 1] << 16 | data[index + 2] << 8 | data[index + 3]);
                dateTime = date + TimeSpan.FromSeconds(time / 1000).ToString();

                /*sw.WriteLine(deviceType.ToString() + ": " + Convert.ToString(data[index + 0], 16) + ","
                    + Convert.ToString(data[index + 1], 16) + "," + Convert.ToString(data[index + 2], 16)
                  + "," + Convert.ToString(data[index + 3], 16) + " " + dateTime);*/

                byte[] record = new byte[singleRecordLength - 4];

                Array.Copy(data, index + 4, record, 0, singleRecordLength - 4);
                RtuRecordKey recordKey = new RtuRecordKey(this.nodeID, deviceType, dateTime);

                lock (reportSignal)
                {
                    try
                    {
                        this.recordQueueData.Add(recordKey, record);
                        this.recordQueueKey.Enqueue(recordKey);

                        Monitor.Pulse(reportSignal);
                    }
                    catch
                    {

                    }

                }
                //Trace.WriteLine("recordQueueData");
                // }
            }
        }

        /// <summary>
        /// 计算值
        /// </summary>
        /// <param name="time">时间戳</param>
        /*
        public int IncreaseTimesCounter(string time)
        {
            throw new System.NotImplementedException();
        }*/

        /// <summary>
        /// 初始化
        /// </summary>
        public void InitSensers()
        {
            foreach (DataCollector dataColletor in senserDictionary.Values)
            {
                /*AsyncMethodCallerReturnBool stopSamplingCaller = new AsyncMethodCallerReturnBool(dataColletor.StopSampling);
                stopSamplingCaller.BeginInvoke(new AsyncCallback(CallbackMethodReturnBool), null);

                AsyncMethodCallerReturnBool setSystemTimeCaller = new AsyncMethodCallerReturnBool(dataColletor.SetSystemTime);
                setSystemTimeCaller.BeginInvoke(new AsyncCallback(CallbackMethodReturnBool), null);

                AsyncMethodCallerReturnInt getSingleRecordLengthCaller = new AsyncMethodCallerReturnInt(dataColletor.GetSingleRecordLength);
                getSingleRecordLengthCaller.BeginInvoke(new AsyncCallback(CallbackMethodReturnInt), null);

                AsyncMethodCallerReturnDeviceType getDeviceTypeCaller = new AsyncMethodCallerReturnDeviceType(dataColletor.GetDeviceType);
                getDeviceTypeCaller.BeginInvoke(new AsyncCallback(CallbackMethodReturnDeviceType), null);*/

                dataColletor.InitSenser(this.initSettings.DeviceRange);
            }
        }

        public DataCenter DataCenter
        {
            get
            {
                return this.dataCenter;
            }
        }
        /*
        public delegate DeviceType AsyncMethodCallerReturnDeviceType();

        public void CallbackMethodReturnDeviceType(IAsyncResult ar)
        {
            AsyncResult result = (AsyncResult)ar;
            AsyncMethodCallerReturnDeviceType caller = (AsyncMethodCallerReturnDeviceType)result.AsyncDelegate;

            caller.EndInvoke(ar);
        }
        
        public delegate void AsyncMethodCaller();

        public void CallbackMethod(IAsyncResult ar)
        {
            AsyncResult result = (AsyncResult)ar;
            AsyncMethodCaller caller = (AsyncMethodCaller)result.AsyncDelegate;

            caller.EndInvoke(ar);
        }

        public delegate bool AsyncMethodCallerReturnBool();

        public void CallbackMethodReturnBool(IAsyncResult ar)
        {
            AsyncResult result = (AsyncResult)ar;
            AsyncMethodCallerReturnBool caller = (AsyncMethodCallerReturnBool)result.AsyncDelegate;

            caller.EndInvoke(ar);
        }

        public delegate int AsyncMethodCallerReturnInt();

        public void CallbackMethodReturnInt(IAsyncResult ar)
        {
            AsyncResult result = (AsyncResult)ar;
            AsyncMethodCallerReturnInt caller = (AsyncMethodCallerReturnInt)result.AsyncDelegate;

            caller.EndInvoke(ar);
        }

        public delegate byte[] AsyncMethodCallerReturnData();

        public void CallbackMethodReturnData(IAsyncResult ar)
        {
            RecordSpliteState recordSpliteState = (RecordSpliteState)ar.AsyncState;
            AsyncResult result = (AsyncResult)ar;
            AsyncMethodCallerReturnData caller = (AsyncMethodCallerReturnData)result.AsyncDelegate;

            byte[] data = caller.EndInvoke(ar);

            if (data != null)
            {
                SplitRecordData(recordSpliteState.DeviceType, data, recordSpliteState.SingleRecordLength);
            }
        }*/
    }

    public class DataRecord
    {
        public string DateTime;

        public byte[] Data;

        public DataRecord(string dateTime, byte[] data)
        {
            this.DateTime = dateTime;
            this.Data = data;
        }
    }
}
