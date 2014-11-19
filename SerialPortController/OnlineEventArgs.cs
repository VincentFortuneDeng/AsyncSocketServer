using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortListener
{
    public class OnlineEventArgs : EventArgs
    {
        public int Address;
        public bool Online;

        public OnlineEventArgs(int address, bool online)
        {
            this.Address = address;
            this.Online = online;
        }
    }

    public class OnworkEventArgs : EventArgs
    {
        public int Address;
        public bool Onwork;

        public OnworkEventArgs(int address, bool onwork)
        {
            this.Address = address;
            this.Onwork = onwork;
        }
    }
}
