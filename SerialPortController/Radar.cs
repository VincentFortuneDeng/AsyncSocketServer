using System;
using System.Collections.Generic;
using System.Text;
using SenserModels;
using System.Diagnostics;
using SenserModels.Entity;
using SenserModels.Common;
using System.Windows.Forms;

namespace SerialPortController
{
    public class Radar : Senser
    {
        #region private const
        /// <summary>
        /// 最大通讯数据长度
        /// </summary>
        private const byte MAX_DATA_LENGTH = 60;
        private const UInt16 BEGIN_OFFSET = 0x0000;
        private const UInt16 END_OFFSET = 0x7BC0;
        private const byte WORD = 2;
        private const byte DOOUBLE_WORD = 4;

        #endregion private const

        #region private variables

        #endregion private variables

        #region public property

        #endregion public property

        #region public enum
        #endregion public enum

        #region  Constructor
        #endregion Constructor

        #region public method
        #endregion public method

        #region private method
        #endregion private method


        //private byte countToRead;
        /// <summary>
        /// 已读记录数
        /// </summary>
        private int recordsRead;
        /// <summary>
        /// 单条数据长度
        /// </summary>
        private byte singleRecordLength;

        public byte SingleRecordLength
        {
            get { return singleRecordLength; }
            set { singleRecordLength = value; }
        }
        /// <summary>
        /// 设备类型
        /// </summary>
        private DeviceType deviceType;

        public DeviceType DeviceType
        {
            get { return deviceType; }
            set { deviceType = value; }
        }
        private int recordCount;
        /// <summary>
        /// 量程
        /// </summary>
        private int deviceRange;

        public int DeviceRange
        {
            get { return deviceRange; }
            set { deviceRange = value; }
        }

        /// <param name="address">地址</param>
        public Radar(byte address, RadarListener listener)
        {
            Reset();

            this.m_listener = listener;

            ((RadarListener)this.m_listener).OfflineEvent += new EventHandler<OnlineEventArgs>(comController_OfflineEvent);

            this.address = address;
        }

        private const byte READ_VAULES = 0x03;
        private const byte WRITE_MUTI_VALUE = 0x10;
        private const byte WRITE_SINGLE_VALUE = 0x06;

        public override bool Ping()
        {
            /*
            //bool lastOnline = this.online;
            bool online = ((CollecterComController)this.comController).SendReturnBool(GenerateAddress(SAMPLING_START_SIGN),
                READ_VAULES, WORD, null);
            Trace.WriteLine("SAMPLING_START_SIGN");
            if (online)
            {
                //Trace.WriteLine("Ping:" + online.ToString());
                if (this.workState == DeviceWorkState.Fault)
                {
                    this.workState = DeviceWorkState.OnReady;
                    RaiseWorkStateEvent();
                }
            }

            return online;*/

            return this.GetRecordCount() != -1;
        }

        /// <summary>
        /// 获得起始地址
        /// </summary>
        private UInt32 GenerateOffset()
        {
            //Trace.WriteLine("Offset:" + (BEGIN_OFFSET + singleRecordLength * recordsRead));
            return (UInt32)(this.address << 16 | (BEGIN_OFFSET + singleRecordLength * recordsRead));
        }

        /// <summary>
        /// 计算读取寄存器数
        /// </summary>
        private byte GenerateCountToRead()
        {
            //Trace.WriteLine("CountToRead" + (byte)(singleRecordLength * (recordCount - recordsRead) % MAX_DATA_LENGTH));
            return (byte)(singleRecordLength * (recordCount - recordsRead) > MAX_DATA_LENGTH ?
                ((int)Math.Truncate((double)(MAX_DATA_LENGTH / singleRecordLength))) * singleRecordLength :
                singleRecordLength * (recordCount - recordsRead));
        }

        /// <summary>
        /// 生成时间数据
        /// </summary>
        private int GenerateTime()
        {
            return (int)DateTime.Now.TimeOfDay.TotalMilliseconds;
        }

        /// <summary>
        /// 获取系统时间
        /// </summary>
        public DateTime GetSystemTime()
        {
            throw new System.NotImplementedException();
        }

        private byte[] GenerateWord(int data)
        {
            return new byte[] { (byte)(data >> 8), (byte)data };
        }

        private byte[] GenerateDoubleWord(int data)
        {
            return new byte[] { (byte)(data >> 24), (byte)(data >> 16), (byte)(data >> 8), (byte)data };
        }

        #region 设置
        /// <summary>
        /// 06时间同步标志寄存器地址
        /// </summary>
        private const UInt16 TIME_SYNC_SIGN = 0x7bd4;
        /// <summary>
        /// 06系统时间寄存器地址              4个字节
        /// </summary>
        private const UInt16 SYSTEM_TIME = 0x7bd6;
        /// <summary>
        /// 06采样时间间隔标志寄存器地址
        /// </summary>
        private const UInt16 SAMPLING_INTERVAL_SIGN = 0x7bde;
        /// <summary>
        /// 06采样时间间隔寄存器地址
        /// </summary>
        private const UInt16 SAMPLING_INTERVAL = 0x7be0;
        /// <summary>
        /// 06采样开始标志寄存器地址
        /// </summary>
        private const UInt16 SAMPLING_START_SIGN = 0x7be8;
        #endregion

        #region 读取
        /// <summary>
        /// 03采样数据计数寄存器地址
        /// </summary>
        private const UInt16 RECORD_COUNT = 0x7bf2;
        /// <summary>
        /// 03设备类型设备信息寄存器地址 4个字节
        /// </summary>
        private const UInt16 DEVICE_TYPE = 0x7bfc;
        /// <summary>
        /// 03设备量程寄存器地址
        /// </summary>
        private const UInt16 DEVICE_RANGE = 0x7c06;
        /// <summary>
        /// 03单条数据长度寄存器地址
        /// </summary>
        private const UInt16 SINGLE_RECORD_LENGTH = 0x7c10;


        private const UInt16 MIN_VALUE = 0x7c24;

        private const UInt16 MAX_VALUE = 0x7c26;

        private const UInt16 DEVICE_RANGE_SIGN = 0x7c28;

        private const UInt16 CURRENT_VALUE = 0x7c2a;
        #endregion

        /// <summary>
        /// 设置采用间隔
        /// </summary>
        /// <param name="samplingInterval">采样间隔</param>
        public bool SetSamplingInterval(int samplingInterval)
        {
            byte[] data = GenerateWord(samplingInterval);
            //Trace.WriteLine("SetSamplingInterval");
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(SAMPLING_INTERVAL),
                 WRITE_SINGLE_VALUE, WORD, data))
            {
                //Trace.WriteLine("SAMPLING_INTERVAL");
                if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(SAMPLING_INTERVAL_SIGN),
                 WRITE_SINGLE_VALUE, WORD, new byte[] { 0x00, 0x01 }))
                {
                    //Trace.WriteLine("SAMPLING_INTERVAL_SIGN");
                    //Trace.WriteLine("SetSamplingInterval");
                    return true;
                }
            }

            return false;
        }

        private UInt32 GenerateAddress(UInt16 commandAddress)
        {
            return (UInt32)(this.address << 16 | commandAddress);
        }


        public bool SetMaxValue(int value)
        {
            //int time = this.GenerateTime();
            byte[] data = GenerateWord(value);
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(MAX_VALUE),
                 WRITE_SINGLE_VALUE, WORD, data))
            {
                return true;
            }

            return false;
        }

        public bool SetMinValue(int value)
        {
            byte[] data = GenerateWord(value);
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(MIN_VALUE),
                 WRITE_SINGLE_VALUE, WORD, data))
            {
                return true;
            }

            return false;
        }

        public UInt16 ReadCurrentValue()
        {
            //byte[] data = GenerateWord(value);
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(DEVICE_RANGE_SIGN),
                 WRITE_SINGLE_VALUE, WORD, new byte[] { 0x00, 0x01 }))
            {
                byte[] values = ((CollecterListener)this.m_listener).SendReturnData(GenerateAddress(CURRENT_VALUE),
                READ_VAULES, WORD, null);
                //Trace.WriteLine("GetRecordCount");
                if (values != null)
                {

                    return GenerateUsignInt(values);
                }
            }

            return 0;
        }

        /// <summary>
        /// 设置系统时间
        /// </summary>
        public bool SetSystemTime()
        {
            int time = this.GenerateTime();
            byte[] data = GenerateDoubleWord(time);
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(SYSTEM_TIME),
                 WRITE_MUTI_VALUE, DOOUBLE_WORD, data))
            {
                //Trace.WriteLine("SYSTEM_TIME");
                if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(TIME_SYNC_SIGN),
                 WRITE_SINGLE_VALUE, WORD, new byte[] { 0x00, 0x01 }))
                {
                    //Trace.WriteLine("TIME_SYNC_SIGN");
                    //Trace.WriteLine("SetSystemTime");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 启动采样
        /// </summary>
        public bool StartSampling()
        {
            bool onwork = ((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(SAMPLING_START_SIGN),
                 WRITE_SINGLE_VALUE, WORD, new byte[] { 0x00, 0x01 });
            //Trace.WriteLine("SAMPLING_START_SIGN");
            if (onwork)
            {
                if (this.workState != DeviceWorkState.OnWork)
                {
                    //Trace.WriteLine("StartSampling");
                    this.workState = DeviceWorkState.OnWork;
                    RaiseWorkStateEvent();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 停止采样
        /// </summary>
        public bool StopSampling()
        {
            bool onwork = ((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(SAMPLING_START_SIGN),
                   WRITE_SINGLE_VALUE, WORD, new byte[] { 0x00, 0x00 });
            //Trace.WriteLine("SAMPLING_START_SIGN 0");
            if (onwork)
            {
                if (this.workState != DeviceWorkState.OnReady)
                {
                    //Trace.WriteLine("StopSampling");
                    this.workState = DeviceWorkState.OnReady;

                    RaiseWorkStateEvent();
                }

                return true;
            }

            return false;
        }

        private int GenerateInt(byte[] data)
        {
            return data[0] << 8 | data[1];
        }

        private UInt16 GenerateUsignInt(byte[] data)
        {
            return (UInt16)(data[0] << 8 | data[1]);
        }

        public bool DetectRunning(int crtRecordCount)
        {
            if (this.recordCount >= crtRecordCount)//数值没有变化
            {
                return false;
            }

            else
            {
                return true;
            }
        }

        public int GetRecordCount()
        {
            byte[] counts = ((CollecterListener)this.m_listener).SendReturnData(GenerateAddress(RECORD_COUNT),
                READ_VAULES, WORD, null);
            //Trace.WriteLine("GetRecordCount");
            if (counts != null)
            {

                int crtRecordCount = GenerateInt(counts);
                //Trace.WriteLine("RecordCount：" + crtRecordCount);
                if (DetectRunning(crtRecordCount))
                {
                    if (this.workState != DeviceWorkState.OnWork)
                    {
                        this.workState = DeviceWorkState.OnWork;

                        RaiseWorkStateEvent();
                    }
                }

                else
                {
                    if (this.workState != DeviceWorkState.OnReady)
                    {
                        this.workState = DeviceWorkState.OnReady;

                        RaiseWorkStateEvent();
                    }
                }
                this.recordCount = crtRecordCount;
                return this.recordCount;
            }

            else
            {
                if (this.workState != DeviceWorkState.Fault)
                {
                    this.workState = DeviceWorkState.Fault;

                    RaiseWorkStateEvent();
                }

                return -1;
            }

        }

        /// <summary>
        /// 获得设备类型
        /// </summary>
        public DeviceType GetDeviceType()
        {
            byte[] types = ((CollecterListener)this.m_listener).SendReturnData(GenerateAddress(DEVICE_TYPE),
                READ_VAULES, DOOUBLE_WORD, null);
            //Trace.WriteLine("DEVICE_TYPE");

            if (types != null)
            {
                //MessageBox.Show("typesNotNull");
                //Trace.WriteLine("DeviceType");
                string devType = string.Empty;
                foreach (byte charAscii in types)
                {
                    if (charAscii != 0)
                    {
                        devType += (char)charAscii;
                    }
                }

                this.deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), devType);
            }

            else
            {
                //MessageBox.Show("typesNull");
                this.deviceType = DeviceType.Unknown;
            }

            return this.deviceType;
        }

        /// <summary>
        /// 设置设备量程
        /// </summary>
        public bool SetDeviceRange(RangeSaveType range)
        {
            byte[] data = GenerateWord(range.FirstValue);
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(DEVICE_RANGE),
                WRITE_SINGLE_VALUE, WORD, data))
            {
                data = GenerateWord(range.SecondValue);
                if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(DEVICE_RANGE + 2),
                WRITE_SINGLE_VALUE, WORD, data))
                {
                    //Trace.WriteLine("DEVICE_RANGE");
                    //Trace.WriteLine("SetDeviceRange");
                    return true;
                }
            }

            return false;
        }

        public bool SetDeviceRangeMax(int value)
        {
            byte[] data = GenerateWord(value);
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(DEVICE_RANGE + 2),
                WRITE_SINGLE_VALUE, WORD, data))
            {
                return true;
            }

            return false;
        }

        public bool SetDeviceRangeMin(int value)
        {
            byte[] data = GenerateWord(value);
            if (((CollecterListener)this.m_listener).SendReturnBool(GenerateAddress(DEVICE_RANGE),
                WRITE_SINGLE_VALUE, WORD, data))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 解析
        /// </summary>
        private DateTime ParseTime(int time)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取记录长度
        /// </summary>
        public int GetSingleRecordLength()
        {
            byte[] length = ((CollecterListener)this.m_listener).SendReturnData(GenerateAddress(SINGLE_RECORD_LENGTH),
                READ_VAULES, WORD, null);
            //Trace.WriteLine("SINGLE_RECORD_LENGTH");
            if (length != null)
            {
                this.singleRecordLength = (byte)GenerateInt(length);
            }

            else
            {
                this.singleRecordLength = 0;
            }
            //Trace.WriteLine("SingleRecordLength：" + this.singleRecordLength);
            return this.singleRecordLength;
        }

        /// <summary>
        /// 获取记录
        /// </summary>
        public byte[] ReadAllRecords()
        {
            this.recordsRead = 0;
            return ReadRealTimeRecord();
        }

        private UInt32 GenerateLastOffset()
        {
            //(UInt32)(this.address << 16 | (BEGIN_OFFSET + singleRecordLength * recordsRead));
            return (UInt32)(this.address << 16 | (BEGIN_OFFSET + singleRecordLength * (recordCount - 1)));
        }

        public byte[] ReadLastRecord()
        {
            this.GetRecordCount();

            byte[] data = ((CollecterListener)this.m_listener).SendReturnData(GenerateLastOffset(),
                     READ_VAULES, singleRecordLength, null);
            //Trace.WriteLine("LastRecord:" + data.Length);
            //Trace.WriteLine("ReadLastRecord");
            return data;
        }

        /// <summary>
        /// 读取最后一条记录
        /// </summary>
        public byte[] ReadRealTimeRecord()
        {
            this.GetRecordCount();

            List<byte> datas = new List<byte>();

            while (this.recordsRead < recordCount)
            {
                byte countToRead = this.GenerateCountToRead();

                if (countToRead != 0)
                {
                    //Trace.WriteLine(string.Format("Address{0} recordsRead{1} recordCount{2}", address, recordsRead, recordCount));
                    byte[] data = ((CollecterListener)this.m_listener).SendReturnData(GenerateOffset(),
                         READ_VAULES, countToRead, null);
                    //Trace.WriteLine("countToRead");
                    if (data != null)
                    {
                        datas.AddRange(data);
                        this.recordsRead += countToRead / singleRecordLength;
                        //Trace.WriteLine(string.Format("Address{0} recordsRead{1}", address, this.recordsRead));
                        //Trace.WriteLine("RealTimeRecord:" + data.Length + " recordsRead:" + recordsRead);
                    }

                    else
                    {
                        break;
                    }
                }
            }
            /*
            int newDataNums = 0;
            foreach (byte[] da in datas)
            {
                newDataNums += da.Length;
            }

            byte[] newDatas = new byte[newDataNums];
            int lastIndex = 0;
            foreach (byte[] data in datas)
            {
                Array.Copy(data, 0, newDatas, lastIndex, data.Length);
                lastIndex += data.Length;
            }*/

            return datas.ToArray();
        }

        public void InitSenser(SerializableDictionary<DeviceType, RangeSaveType> range)
        {
            if (this.m_listener.IsOpen)
            {
                if (this.StopSampling())
                {
                    this.GetDeviceType();

                    this.SetDeviceRange(range[this.DeviceType]);

                    this.SetSamplingInterval(1000);

                    this.SetSystemTime();

                    this.GetSingleRecordLength();
                }
            }
        }

        public void RestartSampling()
        {
            this.recordCount = -1;
            //this.singleRecordLength = 0;
            this.recordsRead = 0;
            //this.deviceType = DeviceType.Unknown;
            //this.online = false;
            //this.onwork = false;
            //this.workState = DeviceWorkState.;
            if (this.m_listener.IsOpen)
            {
                this.StopSampling();

                this.SetSystemTime();

                this.StartSampling();
            }

            //this.GetSingleRecordLength();

            //this.GetDeviceType();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            this.recordCount = -1;
            this.singleRecordLength = 0;
            this.recordsRead = 0;
            this.deviceType = DeviceType.Unknown;
            //this.online = false;
            //this.onwork = false;
            this.workState = DeviceWorkState.Fault;
        }

        //private bool online;
        /*
        public bool Online
        {
            get
            {
                return this.online;
            }

            set
            {
                this.online = value;
            }
        }*/

        //private bool onwork;
        /*
        public bool Onwork
        {
            get
            {
                return this.online;
            }
            set
            {
                this.online = value;
            }
        }*/



        public new event System.EventHandler<WorkStateEventArgs> WorkStateEvent;

        public override void RaiseWorkStateEvent()
        {
            if (this.WorkStateEvent != null)
            {
                this.WorkStateEvent(this,
                    new WorkStateEventArgs(this.address, this.workState, this.deviceType));
            }
        }

        public bool Busy
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
    }
}
