using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortListener
{
    public interface ISenser
    {
        event System.EventHandler<SerialPortListener.WorkStateEventArgs> WorkStateEvent;
    
        SerialPortListener.DeviceWorkState WorkState
        {
            get;
            set;
        }

        /// <summary>
        /// 基础通信
        /// </summary>
        bool ComOn
        {
            get;
        }

        /// <summary>
        /// 地址
        /// </summary>
        byte Address
        {
            get;
        }
        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="data">数据</param>
        bool Ping();

        /// <summary>
        /// 重置
        /// </summary>
        void Reset();

        void RaiseWorkStateEvent();
    }
}
