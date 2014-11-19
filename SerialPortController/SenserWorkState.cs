using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortListener
{
    public enum DeviceWorkState
    {
        Fault = 0x00,
        OnWork = 0x11,
        OnReady = 0x10,
    }
}
