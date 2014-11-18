using System;
using System.Collections.Generic;
using System.Text;
using SenserModels;
using SenserModels.Entity;

namespace SerialPortController
{
    public class RecordSpliteState
    {
        public DeviceType DeviceType;

        public int SingleRecordLength;

        public RecordSpliteState(DeviceType deviceType, int singleRecordLength)
        {
            this.DeviceType = deviceType;
            this.SingleRecordLength = singleRecordLength;
        }
    }
}
