using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SerialPortController
{
    public class DistanceData : EventArgs
    {
        public string DataTime;
        public byte Address;

        public int Battery;

        public bool Stoped;

        public bool Flameouted;

        public string Sign;

        public override string ToString()
        {
            string retString = Address.ToString();
            retString += "," + DataTime;
            retString += "," + this.Stoped;
            retString += "," + Sign.ToString();
            retString += "," + Distance;
            retString += "," + SumDistance;
            retString += "," + Flameouted;
            retString += "," + Battery;

            return retString;
        }

        //public int pulseDistance;

        //ublic DeviceWorkState DeviceWorkState;
        public long SumDistance;

        public int Distance;

        public DistanceData()
        {

        }

        public DistanceData(byte address, string sign, int distance, bool flameouted, int battery)
            : this()
        {
            this.Address = address;
            this.Battery = battery;
            this.Flameouted = flameouted;
            //this.pulseDistance = pulseDistance;
            this.Distance = distance;

            this.Sign = sign;
            //this.DeviceWorkState = deviceWorkState;
        }

    }
}
