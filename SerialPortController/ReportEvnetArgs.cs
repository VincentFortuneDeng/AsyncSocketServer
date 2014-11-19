using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortListener
{
    public class DistanceEventArgs : EventArgs
    {
        public byte Address;

        public int Battery;

        public bool Flameouted;

        public int SumPulseDistance;

        public string Sign;

        public DeviceWorkState DeviceWorkState;

        public int Distance;

        public DistanceEventArgs(byte address, string sign, int distance,
            int sumPulseDistance, bool flameouted, int battery, DeviceWorkState deviceWorkState)
        {
            this.Address = address;
            this.Sign = sign;
            this.Battery = battery;
            this.Flameouted = flameouted;
            this.SumPulseDistance = sumPulseDistance;
            this.Distance = distance;
            this.DeviceWorkState = deviceWorkState;
        }

        public override string ToString()
        {
            return this.Address + " " + this.DeviceWorkState + " :" 
                + this.Sign + this.Distance + "|累计 " + this.SumPulseDistance 
                + "|熄火 " + this.Flameouted + "|电量 " + this.Battery;

        }

    }


}
