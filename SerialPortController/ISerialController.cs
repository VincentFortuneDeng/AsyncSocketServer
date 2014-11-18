using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace SerialPortController
{
    /// <summary>
    /// 设备工作模式
    /// </summary>
    /// <remarks>
    /// 0主动
    /// 1被动
    /// </remarks>
    public enum ReportWorkMode
    {
        /// <summary>
        /// 主动方式
        /// </summary>
        Initiative = 0,
        /// <summary>
        /// 被动方式
        /// </summary>
        Passive = 1
    }

    public interface ISerialController : IDisposable
    {
        /*
        int WaitTime
        {
            get;
            set;
        }*/

        bool IsOpen
        {
            get;
        }

        ReportWorkMode ReportMode
        {
            get;
        }

        bool TimedOut
        {
            get;
            set;
        }

        void Close();

        byte[] CreateByteArrayOfLength(byte[] bArray, int length);

        byte[] Ping(UInt32 address, byte[] data);

        IDataFramePacket Send(UInt32 address, byte type/*命令头*/, byte dataLength/*数据长度*/, byte[] data/*数据内容*/);

        string CreateStringOfLength(string s, int length);

        void Start();

        void Stop();

        bool ChangeSetting(string portName, int baudRate, ReportWorkMode reportMode);

        bool ChangeWorkMode(SerialPortController.ReportWorkMode workMode);
    }

    public abstract class SerialController : ISerialController
    {
        #region protected const
        //protected int waitTime;
        protected const int READ_BUFFER_SIZE = 128;
        protected const int MAX_DATA_LENGTH = 60;
        protected const int MAX_RESPONSE_TIME = 300;
        //protected const int LINE_LENGTH = 16;
        protected const int MAX_PACKET_LENGTH = MAX_DATA_LENGTH + 5;
        protected const byte NORMAL_RESPONSE = 0x40;
        protected const byte NORMAL_REPORT = 0x80;
        protected const byte SPECIAL_RESPONSE = 0xC0;
        protected const byte ERROR_RESPONSE = 0x11;
        //protected const byte KEY_ACTIVITY_REPORT = 0x80;
        //protected const byte FAN_SPEED_REPORT = 0x81;
        //protected const byte TEMPATURE_SENSOR_REPORT = 0x82;
        protected const char FRAME_HEAD = '$';
        protected const char FRAME_TAIL = '@';
        protected const int RESENT_TIMES = 3;

        protected const int READ_WRITE_TIMEOUT = 3000;
        #endregion protected const

        #region protected variables
        protected ushort CRC_SEED = 0xFFFF;
        protected byte[] packetRcvBuffer;
        protected byte[] packetXMitBuffer;
        protected bool timedOut;
        protected Thread receiveThread;
        protected Thread eventThread;
        protected IDataFramePacket responsePacket;
        protected Queue<IDataFramePacket> reportQueue;
        protected Object responseSignal;
        protected Object reportSignal;
        //protected LCDKey _key;
        //protected byte fan1Power, fan2Power, fan3Power, fan4Power;
        protected Boolean disposed = false;
        protected ReportWorkMode reportMode;
        // comPort
        protected SerialPort com;
        #endregion protected variables

        public bool TimedOut
        {
            get { return timedOut; }
            set { timedOut = value; }
        }

        public ReportWorkMode ReportMode
        {
            get { return this.reportMode; }
        }

        /*
        public int WaitTime
        {
            get
            {
                return this.waitTime;
            }

            set
            {
                this.waitTime = value;
            }
        }*/

        public bool IsOpen
        {
            get { return this.com.IsOpen; }
        }

        public void Close()
        {
            Dispose(true);
        }

        public byte[] CreateByteArrayOfLength(byte[] bArray, int length)
        {
            if (length != bArray.Length)
            {
                int dataLength = bArray.Length > length ? length : bArray.Length;
                byte[] newbArray = new byte[length];

                Array.Copy(bArray, newbArray, dataLength);
                bArray = newbArray;
            }

            return bArray;
        }

        public abstract byte[] Ping(UInt32 address, byte[] data);

        public SerialController()
        {

        }

        public SerialController(string portName, int baudRate, ReportWorkMode reportMode, bool discardNull)
            : this()
        {
            // comPort
            com = new SerialPort(portName, baudRate);
            com.Encoding = Encoding.ASCII;
            com.ReadTimeout = READ_WRITE_TIMEOUT;
            com.WriteTimeout = READ_WRITE_TIMEOUT;
            com.DiscardNull = discardNull;
            this.portName = com.PortName;

            //MessageBox.Show(string.Format("ProtName{0},BaudRate{1}",com.PortName, com.BaudRate));
            this.Start();

            //this.waitTime = waitTime;
            this.reportMode = reportMode;
            packetRcvBuffer = new byte[MAX_PACKET_LENGTH];
            packetXMitBuffer = new byte[MAX_PACKET_LENGTH];
            //fan1Power = fan2Power = fan3Power = fan4Power = (byte)0;
            reportQueue = new Queue<IDataFramePacket>();
            responseSignal = new Object();
            reportSignal = new Object();
            eventThread = new Thread(new ThreadStart(ReportEventHandler));
            eventThread.IsBackground = true;
            eventThread.Start();
            if (ReportWorkMode.Initiative == this.reportMode)
            {
                this.eventThreadRun = true;
            }

            receiveThread = new Thread(new ThreadStart(Receive));
            receiveThread.IsBackground = true;
            this.receiveThread.Priority = ThreadPriority.Lowest;
            receiveThread.Start();
            if (this.com.IsOpen)
            {
                //MessageBox.Show("com.IsOpen");
                this.workThreadRun = true;
            }
        }

        public virtual IDataFramePacket Send(UInt32 address, byte type, byte dataLength, byte[] data)
        {
            ushort crc;

            if ((null == data && dataLength != 0) || dataLength > data.Length)
            {
                throw new ArgumentException("发送数据无效");
            }
            //XMit transmit 传输，转送，传达，传导，发射，遗传，传播，发射信号(代号)

            packetXMitBuffer[0] = type;//命令头 
            packetXMitBuffer[1] = dataLength;//数据长度
            if (0 != dataLength)
            {
                Array.Copy(data, 0, packetXMitBuffer, 2, dataLength);
            }

            crc = CRCGenerator.GenerateCRC(packetXMitBuffer, dataLength + 2, CRC_SEED);//生成CRC校验码
            packetXMitBuffer[2 + dataLength + 1] = (byte)(crc >> 8);//高8位
            packetXMitBuffer[2 + dataLength] = (byte)crc;//低8位

            lock (responseSignal)
            {
                responsePacket = null;//清空响应包
                // comPort
                com.Write(packetXMitBuffer, 0, dataLength + 4);
                if (Monitor.Wait(responseSignal, MAX_RESPONSE_TIME))//发送后等待响应
                {
                    return responsePacket;
                }
            }

            return null;//响应超时
        }

        public string CreateStringOfLength(string s, int length)
        {
            if (length < s.Length)
            {
                s = s.Substring(length);
            }

            else if (length > s.Length)
            {
                s = s + (new String(' ', length - s.Length));
            }

            return s;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing && com != null && com.IsOpen)
            {
                //Reset();
                com.Close();
                CloseThreads();

                // Keep us from calling resetting or closing multiple times
                disposed = true;
            }
        }

        protected virtual bool AddPacket(byte[] buffer, int startIndex)
        {
            IDataFramePacket packet = CreatePacket(buffer, startIndex);//创建包
            //CRC16Generator crc16Generator = new CRC16Generator();
            ushort calculatedCRC = CRC16Generator.GenerateCRC(buffer, startIndex, packet.DataLength + 2);//重新计算CRC

            switch (packet.PacketType)
            {
                case DataPacketType.NORMAL_RESPONSE://正常响应包
                    AddResponsePacket(packet);//添加响应包 并通知发送线程
                    break;

                case DataPacketType.NORMAL_REPORT://正常报告包 
                    AddReportPacket(packet);//添加报告包 并通知报告线程
                    break;

                case DataPacketType.SPECIAL_RESPONSE://错误响应包
                    AddResponsePacket(packet);//添加响应包 并通知发送线程
                    AddReportPacket(packet);//添加报告包 并通知报告线程
                    break;
            }

            if (calculatedCRC != packet.CRC)
            {
                Console.WriteLine("CRC ERROR!(CRC 错误): Calculated(计算值) CRC={0} Actual(实际值) CRC={1}",
                    Convert.ToString(calculatedCRC, 16), Convert.ToString(packet.CRC, 16));

                return false;
            }

            return true;
        }

        protected void AddResponsePacket(IDataFramePacket packet)
        {
            lock (responseSignal)
            {
                responsePacket = packet;
                Monitor.Pulse(responseSignal);//通知发送线程
            }
        }

        protected void AddReportPacket(IDataFramePacket packet)
        {
            lock (reportSignal)
            {
                reportQueue.Enqueue(packet);
                Monitor.Pulse(reportSignal);//通知报告线程
            }
        }

        protected void CloseThreads()
        {
            if (receiveThread != null)
            {
                receiveThread.Abort();
            }

            if (eventThread != null)
            {
                eventThread.Abort();
            }
        }

        //二进制字节数据转换为包数据
        protected abstract IDataFramePacket CreatePacket(byte[] buffer, int startIndex);

        protected virtual void ReportEventHandler()
        {
            try
            {
                //报告包引用变量
                IDataFramePacket packet = null;

                //线程循环
                while (true)
                {
                    if (eventThreadRun)
                    {
                        //等待接收线程的报告通知
                        while (null == packet)
                        {
                            lock (reportSignal)
                            {
                                //如果报告队列中已经存在报告 则取出报告包进行处理
                                if (0 != reportQueue.Count)
                                {
                                    packet = reportQueue.Dequeue();
                                }

                                //如果没有报告则等待报告(线程等待)
                                else
                                {
                                    Monitor.Wait(reportSignal);
                                }
                            }
                        }//end 等待接收线程的报告通知
                        RaiseReportEvent(new EventArgs());
                        //处理报告包
                        //switch (packet.Type)
                        //{
                        //case KEY_ACTIVITY_REPORT://键盘激活报告
                        //ReportKeyActivityEventHandler(packet);
                        //break;

                        //case TEMPATURE_SENSOR_REPORT://温度传感器报告
                        //break;
                        //}
                        packet = null;
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
                if (eventThread != null)
                {
                    eventThread = null;
                }
                throw ode;
            }
        }

        public abstract bool SendReturnBool(UInt32 address, byte type, byte dataLength, byte[] data);

        public abstract byte[] SendReturnData(UInt32 address, byte type, byte dataLength, byte[] data);

        protected virtual void RaiseOfflineEvent(OnlineEventArgs e)
        {
            if (this.OfflineEvent != null)
            {
                this.OfflineEvent(this, e);
            }
        }

        /// <summary>
        /// 离线事件
        /// </summary>
        public event System.EventHandler<OnlineEventArgs> OfflineEvent;

        protected abstract void Receive();

        /// <summary>
        /// 串口异常
        /// </summary>
        public event System.EventHandler<ComOnEventArgs> ComOnEvent;

        protected virtual void RaiseComOnEvent(bool comOn)
        {
            if (this.ComOnEvent != null)
            {
                this.ComOnEvent(this, new ComOnEventArgs(this.com.PortName, comOn));
            }
        }

        protected virtual void RaiseReportEvent(EventArgs e)
        {
            if (this.ReportEvent != null)
            {
                this.ReportEvent(this, e);
            }
        }

        public void Stop()
        {
            workThreadRun = false;
            //MessageBox.Show("Stop");
            try
            {
                com.Close();
                RaiseComOnEvent(false);

            }

            catch (Exception)
            {

            }
        }

        public void Start()
        {
            //MessageBox.Show(string.Format("ProtName{0},BaudRate{1}", com.BaudRate));
            try
            {
                
                com.Open();

                //if (this.com.IsOpen)
                //{
                RaiseComOnEvent(true);
                workThreadRun = true;
                //MessageBox.Show("com.Open()");
                //}
            }

            catch (Exception e)
            {
                workThreadRun = false;
                MessageBox.Show(e.Message);
                RaiseComOnEvent(false);
            }
        }

        protected bool workThreadRun;

        protected bool eventThreadRun;

        /// <summary>
        /// 响应事件
        /// </summary>
        /*public event System.EventHandler<ResponseEventArgs> Response;*/

        /// <summary>
        /// 报告事件
        /// </summary>
        public event System.EventHandler<EventArgs> ReportEvent;

        public bool ChangeSetting(string portName, int baudRate, ReportWorkMode reportMode)
        {
            this.Stop();
            this.com.PortName = portName;
            this.com.BaudRate = baudRate;
            if (this.reportMode != reportMode)
            {
                this.reportMode = reportMode;
                switch (this.reportMode)
                {
                    case ReportWorkMode.Initiative:
                        //this.reportQueue.Clear();
                        //this.eventThread = new Thread(new ThreadStart(ReportEventHandler));
                        //this.eventThread.IsBackground = true;
                        this.eventThreadRun = true;
                        break;

                    case ReportWorkMode.Passive:
                        this.eventThreadRun = false;
                        //this.eventThread = null;
                        this.reportQueue.Clear();
                        break;

                    default:

                        break;
                }
            }
            this.Start();

            return true;
        }

        public bool ChangeWorkMode(ReportWorkMode reportMode)
        {
            this.Stop();

            if (this.reportMode != reportMode)
            {
                this.reportMode = reportMode;
                switch (this.reportMode)
                {
                    case ReportWorkMode.Initiative:
                        //this.reportQueue.Clear();

                        //this.eventThread = new Thread(new ThreadStart(ReportEventHandler));
                        //this.eventThread.IsBackground = true;
                        this.eventThreadRun = true;
                        break;

                    case ReportWorkMode.Passive:
                        this.eventThreadRun = false;
                        //this.eventThread = null;
                        this.reportQueue.Clear();
                        break;

                    default:

                        break;
                }
            }
            this.Start();

            return true;
        }

        public string PortName
        {
            get
            {
                return this.portName;
            }
        }

        protected string portName;
    }

    public class ComOnEventArgs : EventArgs
    {
        public string PortName;
        public bool ComOn;
        public ComOnEventArgs(string portName, bool comOn)
        {
            this.PortName = portName;
            this.ComOn = comOn;
        }

        public override string ToString()
        {
            return this.PortName + (this.ComOn ? ":打开" : ":关闭");
        }
    }
}
