using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortController
{
    public enum RtuCommandType
    {
        #region 设置
        /// <summary>
        /// 06时间同步标志寄存器地址
        /// </summary>
        TIME_SYNC_SIGN = 0x7bd4,
        /// <summary>
        /// 06系统时间寄存器地址              4个字节
        /// </summary>
        SYSTEM_TIME = 0x7bd6,
        /// <summary>
        /// 06采样时间间隔标志寄存器地址
        /// </summary>
        SAMPLING_INTERVAL_SIGN = 0x7bde,
        /// <summary>
        /// 06采样时间间隔寄存器地址
        /// </summary>
        SAMPLING_INTERVAL = 0x7be0,
        /// <summary>
        /// 06采样开始标志寄存器地址
        /// </summary>

        SAMPLING_START_SIGN = 0x7be8,

        MIN_VALUE = 0x7c24,

        MAX_VALUE = 0x7c26,

        DEVICE_RANGE_SIGN = 0x7c28,

        CURRENT_VALUE =0x7c2A,

        #endregion

        #region 读取
        /// <summary>
        /// 03采样数据计数寄存器地址
        /// </summary>
        RECORD_COUNT = 0x7bf2,
        /// <summary>
        /// 03设备类型设备信息寄存器地址 4个字节
        /// </summary>
        DEVICE_TYPE = 0x7bfc,
        /// <summary>
        /// 03设备量程寄存器地址
        /// </summary>
        DEVICE_RANGE = 0x7c06,
        /// <summary>
        /// 03单条数据长度寄存器地址
        /// </summary>
        SINGLE_RECORD_LENGTH = 0x7c10,
        #endregion
    }
}
