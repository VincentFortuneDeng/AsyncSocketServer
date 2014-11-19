using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SerialPortController
{
    public enum PulseAngle
    {
        Angle30 = 1,
        Angle45 = 2,
        Angle90 = 3,
        Angle180 = 4
    }

    public class DistanceSenser : Senser
    {
        /// <summary>
        /// 符号
        /// </summary>
        private string sign;
        /// <summary>
        /// 最后一次通讯距离
        /// </summary>
        private int lastDistance;
        /// <summary>
        /// 累计距离
        /// </summary>
        private long sumDistance;
        /// <summary>
        /// 运行状态
        /// </summary>
        //private RunStateType runState;
        /// <summary>
        /// 电量
        /// </summary>
        private int battery;
        /// <summary>
        /// 熄火
        /// </summary>
        private bool flameouted;
        private bool stoped;



        public bool Stoped
        {
            get { return stoped; }
            set { stoped = value; }
        }

        /// <param name="deviceAddress">地址</param>
        /// <param name="comController">通讯控制器</param>
        public DistanceSenser(byte address, DADListener comController)
        {
            Reset();

            this.address = address;

            this.m_listener = comController;

            this.m_listener.OfflineEvent += new EventHandler<OnlineEventArgs>(comController_OfflineEvent);

        }

        /// <summary>
        /// 运行状态
        /// </summary>
        /*public RunStateType RunState
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }*/

        /// <summary>
        /// 最后距离
        /// </summary>
        public int LastDistance
        {
            get
            {
                return this.lastDistance;
            }
            set
            {
                this.lastDistance = value;
            }
        }

        /// <summary>
        /// 符号
        /// </summary>
        public string Sign
        {
            get
            {
                return this.sign;
            }

            set
            {
                this.sign = value;
            }
        }

        /// <summary>
        /// 累计距离
        /// </summary>
        public long SumDistance
        {
            get
            {
                return this.sumDistance;
            }
            set
            {
                this.sumDistance = value;
            }
        }

        /// <summary>
        /// 熄火
        /// </summary>
        public bool Flameouted
        {
            get
            {
                return this.flameouted;
            }
            set
            {
                this.flameouted = value;
            }
        }

        /// <summary>
        /// 电量
        /// </summary>
        public int Battery
        {
            get
            {
                return this.battery;
            }

            set
            {
                this.battery = value;
            }
        }

        /// <summary>
        /// 修改设备地址
        /// </summary>
        /// <param name="desAddress">目标地址</param>
        public bool SetAddress(string destAddress)
        {
            byte type = 1;

            return ((DADListener)this.m_listener).SendReturnBool(address, type,destAddress);
        }

        /// <summary>
        /// 设置脉冲角度
        /// </summary>
        /// <param name="SetWorkMode">脉冲角度</param>
        public bool SetPulseAngle(PulseAngle angle)
        {
            byte type = 4;

            return ((DADListener)this.m_listener).SendReturnBool(address, type, ((byte)angle).ToString());
        }

        /// <summary>
        /// 设置工作模式
        /// </summary>
        /// <param name="workMode">工作模式</param>
        public bool SetWorkMode(ReportWorkMode workMode)
        {
            byte type = 2;

            return ((DADListener)this.m_listener).SendReturnBool(address, type, ((byte)workMode).ToString());
        }

        /// <summary>
        /// 设置通讯频率
        /// </summary>
        /// <param name="frequency">频率</param>
        public bool SetWirelessFrequency(string frequency)
        {
            byte type = 3;

            return ((DADListener)this.m_listener).SendReturnBool(address, type, frequency);
        }

        /// <summary>
        /// 获得当前输出数据
        /// </summary>
        public override string ToString()
        {
            throw new System.NotImplementedException();
        }

        public bool InsertRecord()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(@".\" + this.Address + ".dsd", false, Encoding.ASCII))
                {
                    sw.WriteLine(this.ToString());
                    sw.Close();

                    return true;
                }
            }

            catch
            {
                return false;
            }
        }

        public override bool Ping()
        {
            byte type = 0;

            bool online = ((DADListener)this.m_listener).SendReturnBool(address, type, "");

            //this.workState = online ? DeviceWorkState.OnWork : DeviceWorkState.Fault;

            if (online)
            {
                //Trace.WriteLine("Ping:" + online.ToString());
                if (this.workState == DeviceWorkState.Fault)
                {
                    this.workState = DeviceWorkState.OnWork;
                    RaiseWorkStateEvent();
                }
            }

            return online;
        }

        public override void RaiseWorkStateEvent()
        {
            if (this.WorkStateEvent != null)
            {
                this.WorkStateEvent(this,
                    new WorkStateEventArgs(this.address, this.workState, null));
            }
        }

        /// <summary>
        /// 读取最后一条记录
        /// </summary>
        public DistanceData ReadRealTimeRecord()
        {
            byte type = 0;
            return ((DADListener)this.m_listener).SendReturnData(this.address, type, "");
        }

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            this.battery = 0;
            this.flameouted = false;
            this.lastDistance = -1;
            this.SumDistance = 0;
            this.sign = "-";
            this.workState = DeviceWorkState.Fault;
        }

        public new event System.EventHandler<WorkStateEventArgs> WorkStateEvent;
    }

    public enum RunStateType
    {
        Backward,
        Forward,
        Stopped
    }
}
