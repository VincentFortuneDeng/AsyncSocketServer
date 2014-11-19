using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortListener
{
    public enum RtuReadWriteType
    {
        /// <summary>
        /// 写单值
        /// </summary>
        WriteSingleValue = 0x06,
        /// <summary>
        /// 写多值
        /// </summary>
        WriteMultiValue = 0x10,
        /// <summary>
        /// 读数据
        /// </summary>
        ReadValues = 0x03,
    }
}
