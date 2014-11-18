using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortController
{
    public interface ISenser
    {
        event System.EventHandler<SerialPortController.WorkStateEventArgs> WorkStateEvent;
    
        SerialPortController.DeviceWorkState WorkState
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
