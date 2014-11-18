using System;
using System.Collections.Generic;
using System.Text;

namespace SenserModels.Entity
{
    public class RangeSaveType
    {
        public DeviceType Device;

        public int FirstValue;

        public int SecondValue;

        public RangeSaveType()
        {
            this.Device = DeviceType.ELEP;
            this.FirstValue = 0;
            this.SecondValue = 0;
        }

        public RangeSaveType(DeviceType device, int firstValue, int secondValue)
        {
            this.Device = device;
            this.FirstValue = firstValue;
            this.SecondValue = secondValue;
        }
    }
}
