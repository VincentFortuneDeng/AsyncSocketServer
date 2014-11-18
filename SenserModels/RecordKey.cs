using System;
using System.Collections.Generic;
using System.Text;
using SenserModels.Entity;

namespace SenserModels
{
    public class DistanceRecordKey
    {
        public byte Address;
        public string DataDateTime;

        public DistanceRecordKey(byte address, string dateTime)
        {
            this.Address = address;

            this.DataDateTime = dateTime;
        }
    }

    public class RtuRecordKey
    {
        public string NodeID;
        public DeviceType DeviceType;
        public string DataDateTime;

        public RtuRecordKey(string nodeID,DeviceType deviceType, string dateTime)
        {
            this.NodeID = nodeID;
            this.DeviceType = deviceType;
            this.DataDateTime = dateTime;
        }
    }
}
