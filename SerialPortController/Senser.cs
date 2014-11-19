using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortListener
{
    public abstract class Senser : ISenser
    {
        /// <summary>
        /// 地址
        /// </summary>
        protected byte address;

        /// <summary>
        /// 通讯控制器
        /// </summary>
        protected SerialListener m_listener;

        protected DeviceWorkState workState;

        public event EventHandler<WorkStateEventArgs> WorkStateEvent;

        public DeviceWorkState WorkState
        {
            get
            {
                return this.workState;
            }
            set
            {
                this.workState = value;
            }
        }

        public bool ComOn
        {
            get { return m_listener.IsOpen; }
        }

        public byte Address
        {
            get { return this.address; }
        }

        public abstract bool Ping();

        public abstract void Reset();

        public abstract void RaiseWorkStateEvent();

        protected virtual void comController_OfflineEvent(object sender, OnlineEventArgs e)
        {
            if (e.Address == this.address)
            {
                if (this.workState != DeviceWorkState.Fault)
                {
                    this.workState = DeviceWorkState.Fault;
                    RaiseWorkStateEvent();
                }
            }
        }
    }
}
