using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SenserModels;
using System.IO;
using System.Windows.Forms;
using SenserModels.Entity;

namespace SerialPortController
{
    public class DADController : SenserController
    {
        /// <summary>
        /// 距离数据队列
        /// </summary>
        private Queue<DistanceData> recordDataQueue;
        private Dictionary<byte, byte> addressMapping;
        private string mapping;

        public string Mapping
        {
            get { return mapping; }
            set
            {
                mapping = value;
                Properties.Settings.Default.AddressMapping = mapping;
                Properties.Settings.Default.Save();
            }
        }
        /// <summary>
        /// 距离传感器列表
        /// </summary>
        //private new Dictionary<byte, SerialPortController.DistanceSenser> senserDictionary;

        /// <summary>
        /// 数据键队列
        /// </summary>
        //private Queue<DistanceRecordKey> recordQueueKey;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DADController(ReportWorkMode workMode, params int[] pollInterval)
            : base(workMode, pollInterval)
        {

        }

        protected override void InitMembers()
        {
            this.recordDataQueue = new Queue<DistanceData>();
        }

        /// <summary>
        /// 输出线程
        /// </summary>
        protected override void ReportEventHandler()
        {
            try
            {
                //DistanceRecordKey recordKey = null;
                DistanceData recordData = null;
                //DistanceSenser distanceSenser = null;

                //线程循环
                while (true)
                {
                    //等待接收线程的报告通知
                    while (null == recordData)
                    {
                        lock (reportSignal)
                        {
                            //如果报告队列中已经存在报告 则取出报告包进行处理
                            if (0 != recordDataQueue.Count)
                            {
                                recordData = recordDataQueue.Dequeue();
                            }

                            //如果没有报告则等待报告(线程等待)
                            else
                            {
                                //this.dataCenter.UpdateCollectingData();

                                Monitor.Wait(reportSignal);
                            }
                        }
                    }//end 等待接收线程的报告通知
                    //recordData = this.recordDataQueue[recordKey.Address][recordKey.DataDateTime];
                    SplitRecordData(ref recordData);

                    InsertRecord(recordData);

                    RaiseReportEvent(recordData);
                    //this.dataCenter.InsertRecord(recordKey, this.recordQueueData[recordKey.DeviceType][recordKey.DataDateTime]);

                    ////处理报告包
                    //switch (packet.Type)
                    //{

                    //}
                    recordData = null;
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

        private bool InsertRecord(DistanceData distanceData)
        {
            try
            {
                //using (StreamWriter sw = new StreamWriter(@".\" + this.addressMapping[distanceData.Address] + ".dsd", false, Encoding.ASCII))
                using (StreamWriter sw = new StreamWriter(@".\" + distanceData.Address + ".dsd", false, Encoding.ASCII))
                {
                    sw.WriteLine(distanceData.ToString());
                    sw.Close();

                    return true;
                }
            }

            catch
            {
                return false;
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
                        DistanceData data = new DistanceData();

                        this.currentSenser = (DistanceSenser)this.NextSenser();
                        if (this.currentSenser != null)
                        {

                            switch (this.currentSenser.WorkState)
                            {
                                case DeviceWorkState.OnWork:
                                    data = ((DistanceSenser)this.currentSenser).ReadRealTimeRecord();

                                    if (data != null)
                                    {
                                        lock (reportSignal)
                                        {
                                            this.recordDataQueue.Enqueue(data);

                                            Monitor.Pulse(reportSignal);
                                        }
                                    }
                                    break;

                                case DeviceWorkState.Fault:
                                    ((DistanceSenser)this.currentSenser).Ping();

                                    break;

                                default:

                                    break;
                            }

                            Thread.Sleep(this.PollInterval);

                        }
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

        /// <summary>
        /// 创建传感器列表
        /// </summary>
        protected override void AddSenser(SerialListener dadComController, string addressList)
        {
            string[] addresses = addressList.Split(',');
            foreach (string address in addresses)
            {
                byte byteAddress = Convert.ToByte(address, 10);
                DistanceSenser distanceSenser = new DistanceSenser(byteAddress, (DADListener)dadComController);
                distanceSenser.WorkStateEvent += new EventHandler<WorkStateEventArgs>(Senser_WorkStateEvent);
                try
                {
                    this.SenserDictionary.Add(byteAddress, distanceSenser);
                }

                catch (ArgumentException e)
                {
                    MessageBox.Show(byteAddress + " 设备地址重复 :" + e.Message);
                }

                //this.recordDataQueue.Add(byteAddress, new Dictionary<string, DistanceData>());
            }

        }



        /// <summary>
        /// 设置地址
        /// </summary>
        /// <param name="srcAddress">原始地址</param>
        /// <param name="destAddress">目标地址</param>
        public bool SetAddress(byte srcAddress, string destAddress)
        {
            if (this.currentSenser != null)
            {
                return ((DistanceSenser)this.senserDictionary[srcAddress]).SetAddress(destAddress);
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// 设置脉冲角度
        /// </summary>
        /// <param name="deviceAddress">地址</param>
        /// <param name="angle">角度</param>
        public bool SetPulseAngle(byte address, PulseAngle angle)
        {
            if (this.currentSenser != null)
            {
                return ((DistanceSenser)this.senserDictionary[address]).SetPulseAngle(angle);
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="deviceAddress">地址</param>
        /// <param name="frequency">频率</param>
        public bool SetWirelessFrequency(byte address, string frequency)
        {
            if (this.currentSenser != null)
            {
                return ((DistanceSenser)this.senserDictionary[address]).SetWirelessFrequency(frequency);
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// 设置工作模式
        /// </summary>
        /// <param name="deviceAddress">地址</param>
        /// <param name="workMode">工作模式</param>
        public bool SetWorkMode(byte address, ReportWorkMode workMode)
        {
            if (this.currentSenser != null)
            {
                return ((DistanceSenser)this.senserDictionary[address]).SetWorkMode(workMode);
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// 添加通讯控制器
        /// </summary>
        /// <param name="setting">设置</param>
        protected override SerialListener AddComController(string setting)
        {
            string[] comSettings = setting.Split(',');
            DADListener dadComController = new DADListener(comSettings[0],
                int.Parse(comSettings[1]), this.workMode, true);

            dadComController.ComOnEvent += new EventHandler<ComOnEventArgs>(ComController_ComOnEvent);

            this.m_listenerDictionary.Add(comSettings[0], dadComController);

            return dadComController;
        }

        protected override void ComController_ReportEvent(object sender, EventArgs e)
        {
            DistanceData recordData = (DistanceData)e;
            SplitRecordData(ref recordData);

            InsertRecord(recordData);

            RaiseReportEvent(recordData);
        }

        //private override void ComController_ComOnEvent(object sender, ComOnEventArgs e)
        //{
        //    RaiseComOnEvent(e);
        //}



        /// <summary>
        /// 获得轮询下一传感器
        /// </summary>
        protected override ISenser NextSenser()
        {
            int i = 0;
            DistanceSenser retDistanceSenser = null;
            foreach (DistanceSenser distanceSenser in this.SenserDictionary.Values)
            {
                if (indexSenser == i++ && distanceSenser.ComOn)
                {
                    retDistanceSenser = distanceSenser;
                    break;
                }
            }
            indexSenser = ++indexSenser % this.SenserDictionary.Count;

            return retDistanceSenser;
        }

        protected override void LoadSetting()
        {
            this.baseSettings = SerialPortController.Properties.Settings.Default.BaseSetting;
            this.addressMapping = new Dictionary<byte, byte>();
            string[] addressMappingSetting = SerialPortController.Properties.Settings.Default.AddressMapping.Split(';');
            foreach (string ams in addressMappingSetting)
            {
                if (!string.IsNullOrEmpty(ams))
                {
                    string[] mapping = ams.Split(':');
                    this.addressMapping.Add(Convert.ToByte(mapping[0]), Convert.ToByte(mapping[1]));
                }
            }
            this.mapping = SerialPortController.Properties.Settings.Default.AddressMapping;
        }

        /// <summary>
        /// 拆解数据
        /// </summary>
        /// <param name="data">要拆分的数据 返回包含时间的数据</param>
        protected void SplitRecordData(ref DistanceData recordData)
        {
            try
            {
                DistanceSenser distanceSenser = (DistanceSenser)this.SenserDictionary[recordData.Address];
                if (distanceSenser.LastDistance > 0)
                {
                    if (recordData.Distance == distanceSenser.LastDistance)
                    {
                        recordData.Sign = distanceSenser.Sign;//方向

                        distanceSenser.Stoped = true;
                        recordData.Stoped = true;

                        recordData.SumDistance = distanceSenser.SumDistance;
                    }

                    else
                    {
                        distanceSenser.Stoped = false;
                        recordData.Stoped = false;

                        //distanceSenser.Sign = recordData.Sign;//方向

                        int distanceAbs = Math.Abs(distanceSenser.LastDistance - recordData.Distance);

                        if (distanceAbs < 10000 - distanceAbs)
                        {
                            distanceSenser.Sign = distanceSenser.LastDistance < recordData.Distance ? "+" : "-";
                        }

                        else
                        {
                            distanceSenser.Sign = distanceSenser.LastDistance > recordData.Distance ? "+" : "-";
                        }

                        recordData.Sign = distanceSenser.Sign;//方向

                        distanceSenser.SumDistance += distanceAbs < 10000 - distanceAbs ? distanceAbs : 10000 - distanceAbs;

                        recordData.SumDistance = distanceSenser.SumDistance;
                    }

                    distanceSenser.Battery = recordData.Battery;
                    distanceSenser.Flameouted = recordData.Flameouted;
                }

                else
                {
                    distanceSenser.Stoped = true;
                    recordData.Stoped = true;

                    distanceSenser.Sign = "-";
                    recordData.Sign = "-";

                    distanceSenser.Flameouted = false;
                    recordData.Flameouted = false;

                    distanceSenser.Battery = recordData.Battery;
                }


                distanceSenser.LastDistance = recordData.Distance;
            }

            catch
            {

            }
        }

        /// <summary>
        /// 拆解数据
        /// </summary>
        /// <param name="data">要拆分的数据 返回包含时间的数据</param>
        protected override void SplitRecordData(DeviceType deviceType, byte[] data, int singleRecordLength)
        {
            throw new NotImplementedException();
        }

    }
}
