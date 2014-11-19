using System;
using System.Collections.Generic;
using System.Text;
using SenserModels;

namespace SerialPortListener
{
    public class WorkStateEventArgs : EventArgs
    {
        public object State;
        public byte DeviceAddress;
        public DeviceWorkState WorkState;

        public WorkStateEventArgs(byte deviceAddress, DeviceWorkState workState, object deviceType)
        {
            this.DeviceAddress = deviceAddress;
            this.State = deviceType;
            this.WorkState = workState;
        }

        public override string ToString()
        {
            return this.DeviceAddress + ":" + this.WorkState;
        }

    }
}
