using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortController
{
    public enum PollModeType
    {
        /// <summary>
        /// 实时
        /// </summary>
        REAL_TIME = 0x01,
        /// <summary>
        /// 全部数据
        /// </summary>
        ALL = 0x10,
        /// <summary>
        /// 不采集
        /// </summary>
        None = 0x00,
        /// <summary>
        /// 全部切换到实时
        /// </summary>
        //ALL_TO_REAL_TIME = 0x11,

        LAST_RECORD = 0x100
    }
}
