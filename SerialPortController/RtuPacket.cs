using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortController
{
    public class RtuPacket:IDataFramePacket
    {
        private byte type;
        private byte dataLength;
        private byte[] data;
        private ushort crc;

        private const byte NORMAL_RESPONSE = 0x00;
        private const byte NORMAL_REPORT = 0xe0;
        private const byte ERROR_RESPONSE = 0xC0;
        public RtuPacket(byte address,byte type, byte dataLength, byte[] data)
        {
            this.deviceAddress = address;
            this.type = type;
            this.dataLength = dataLength;
            this.data = data;
        }

        public RtuPacket(byte address, byte type, byte dataLength, byte[] data, ushort crc)
        {
            this.deviceAddress = address;
            this.type = type;
            this.dataLength = dataLength;
            this.data = data;
            this.crc = crc;
        }
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("DeviceAddress: " + Convert.ToString(deviceAddress, 16) + "\n");
            sb.Append("Type: " + Convert.ToString(type, 16) + "\n");
            sb.Append("DataLength: " + Convert.ToString(dataLength, 16) + "\n");
            sb.Append("Data: ");
            for (int i = 0; i < dataLength; i++)
            {
                sb.Append(Convert.ToString(data[i], 16) + ", ");
            }

            sb.Append("\n");
            sb.Append("CRC: " + Convert.ToString(crc, 16) + "\n");
            return sb.ToString();
        }
        public byte Type
        {
            get
            {
                return type;
            }
        }

        public SerialPortController.DataPacketType PacketType
        {
            get
            {
                switch (type & 0xe0)
                {
                    case NORMAL_RESPONSE:
                        return DataPacketType.NORMAL_RESPONSE;

                    case NORMAL_REPORT:
                        return DataPacketType.NORMAL_REPORT;

                    /*case ERROR_RESPONSE:
                        return DataPacketType.ERROR_RESPONSE;*/

                    default:
                        throw new InvalidOperationException("Unexpected Packet Type: " + System.Convert.ToString(type, 16));
                }
            }
        }

        public byte DataLength
        {
            get
            {
                return dataLength;
            }
        }
        public byte[] Data
        {
            get
            {
                return data;
            }
        }
        public ushort CRC
        {
            get
            {
                return crc;
            }
        }

        /// <summary>
        /// 地址
        /// </summary>
        private byte deviceAddress;

        /// <summary>
        /// 地址
        /// </summary>
        public byte Address
        {
            get
            {
                return this.deviceAddress;
            }
        }


        byte IDataFramePacket.Type
        {
            get
            {
                return this.type;
            }
        }

        public string[] GetDataStrings()
        {
            throw new NotImplementedException();
        }
    }
}
