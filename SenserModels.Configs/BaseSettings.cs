using System;
using System.Collections.Generic;
using System.Text;
using SenserModels.Common;
using SenserModels.Entity;

namespace SenserModels.Config
{
    public class BaseSettings:IConfigInfo
    {
        public BaseSettings()
        {
            this.collectCycle = 1000;
            this.collectIntervalue = 10;

            this.dataDate = DateTime.Now.ToString("yyyy-MM-dd");

            this.deviceRange = new SerializableDictionary<DeviceType, RangeSaveType>();
            deviceRange.Add(DeviceType.ELEP, new RangeSaveType(DeviceType.ELEP, 6000, 3000));
            deviceRange.Add(DeviceType.PUIP, new RangeSaveType(DeviceType.PUIP, 10, 0));
            deviceRange.Add(DeviceType.PUOP, new RangeSaveType(DeviceType.PUOP, 300, 0));
            deviceRange.Add(DeviceType.ULTF, new RangeSaveType(DeviceType.ULTF, 0, 1000));

            this.stationID = "";

        }

        private SerializableDictionary<DeviceType, RangeSaveType> deviceRange;
        public SerializableDictionary<DeviceType, RangeSaveType> DeviceRange
        {
            get
            {
                return this.deviceRange;
            }

            set
            {
                this.deviceRange = value;
            }
        }
        private string stationID;

        public string StationID
        {
            get
            {
                return this.stationID;
            }

            set
            {
                this.stationID = value;
            }
        }

        private string motorID;

        public string MotorID
        {
            get
            {
                return this.motorID;
            }

            set
            {
                this.motorID = value;
            }
        }

        private string dataDate;

        public string DataDate
        {
            get { return dataDate; }
            set { dataDate = value; }
        }

        private int collectIntervalue;

        public int CollectIntervalue
        {
            get { return collectIntervalue; }
            set { collectIntervalue = value; }
        }

        private int collectCycle;

        public int CollectCycle
        {
            get { return collectCycle; }
            set { collectCycle = value; }
        }
    }
}
