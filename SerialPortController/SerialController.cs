using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
namespace SerialPortController
{
    public class FramePacket : IDataFramePacket
    {
        private byte type;
        private byte dataLength;
        private byte[] data;
        private ushort crc;
        //public enum LCDPacketType { NORMAL_RESPONSE, NORMAL_REPORT, ERROR_RESPONSE };
        private const byte NORMAL_RESPONSE = 0x00;
        private const byte NORMAL_REPORT = 0xf0;
        private const byte ERROR_RESPONSE = 0xC0;
        public FramePacket(byte type, byte dataLength, byte[] data)
        {
            this.type = type;
            this.dataLength = dataLength;
            this.data = data;
        }

        public FramePacket(byte type, byte dataLength, byte[] data, ushort crc)
        {
            this.type = type;
            this.dataLength = dataLength;
            this.data = data;
            this.crc = crc;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

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

        public DataPacketType PacketType
        {
            get
            {
                switch (type & 0xf0)
                {
                    case NORMAL_RESPONSE:
                        return DataPacketType.NORMAL_RESPONSE;

                    case NORMAL_REPORT:
                        return DataPacketType.NORMAL_REPORT;

                    case ERROR_RESPONSE:
                        return DataPacketType.ERROR_RESPONSE;

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

        byte IDataFramePacket.Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte Address
        {
            get { throw new NotImplementedException(); }
        }

        byte[] IDataFramePacket.Data
        {
            get { throw new NotImplementedException(); }
        }

        string[] IDataFramePacket.GetDataStrings()
        {
            throw new NotImplementedException();
        }


        public string[] GetDataStrings()
        {
            throw new NotImplementedException();
        }
    }

    public enum DataPacketType
    {
        NORMAL_RESPONSE = 0x40,
        NORMAL_REPORT = 0x80,
        SPECIAL_RESPONSE = 0xC0,
        ERROR_RESPONSE = 0x00
    };

    public interface IDataFramePacket
    {
        string ToString();
        byte Type { get; }
        DataPacketType PacketType { get; }
        byte Address { get; }
        byte[] Data { get; }
        string[] GetDataStrings();
        ushort CRC { get; }
        byte DataLength { get; }
    }

    public class DistancePacket : IDataFramePacket
    {
        private byte deviceAddress;
        private byte type;
        //private byte singleRecordLength;
        //private byte[] data;
        //private ushort crc;

        private const byte NORMAL_RESPONSE = 0x00;
        private const byte NORMAL_REPORT = 0xf0;
        private const byte ERROR_RESPONSE = 0xC0;
        public DistancePacket(byte address, byte type, string stringData)
        {
            this.deviceAddress = address;
            this.type = type;
            this.stringData = stringData;
            this.boolData = true;
        }

        public DistancePacket(byte address, byte type, bool boolData)
        {
            this.deviceAddress = address;
            this.type = type;
            this.boolData = boolData;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("DeviceAddress: " + Convert.ToString(deviceAddress, 10) + "\n");
            sb.Append("Type: " + Convert.ToString(type, 16) + "\n");
            sb.Append("Data: " + stringData + "\n");
            //sb.Append("CRC: " + Convert.ToString(crc, 16) + "\n");

            return sb.ToString();
        }

        public byte Type
        {
            get
            {
                return type;
            }
        }

        public DataPacketType PacketType
        {
            get
            {
                switch (type & 0xf0)
                {
                    case NORMAL_RESPONSE:
                        return DataPacketType.NORMAL_RESPONSE;

                    case NORMAL_REPORT:
                        return DataPacketType.NORMAL_REPORT;

                    case ERROR_RESPONSE:
                        return DataPacketType.SPECIAL_RESPONSE;

                    default:
                        throw new InvalidOperationException("Unexpected Packet Type: " + System.Convert.ToString(type, 16));
                }
            }
        }

        public byte Address
        {
            get
            {
                return deviceAddress;
            }
        }

        public byte[] Data
        {
            get
            {
                return null;
            }
        }

        public ushort CRC
        {
            get
            {
                return 0xffff;
            }
        }

        public byte DataLength
        {
            get { throw new NotImplementedException(); }
        }

        public string[] GetDataStrings()
        {
            throw new NotImplementedException();
        }

        private string stringData;

        public string StringData
        {
            get { return stringData; }
            set { stringData = value; }
        }

        private bool boolData;
        public bool BoolData
        {
            get
            {
                return boolData;
            }

            set
            {
                this.boolData = value;
            }
        }
    }

    public class CRC16Generator
    {
        //private const int CRC_LEN = 2;

        // Table of CRC values for high-order byte
        private static byte[] crcLookupTableHi = new byte[]
        {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40

        };

        // Table of CRC values for low-order byte
        private static byte[] crcLookupTableLo = new byte[]
        {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 
            0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 
            0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 
            0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 
            0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4, 
            0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3, 
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 
            0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 
            0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 
            0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 
            0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED, 
            0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26, 
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 
            0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 
            0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 
            0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 
            0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 
            0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5, 
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 
            0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 
            0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 
            0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 
            0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B, 
            0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C, 
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 
            0x43, 0x83, 0x41, 0x81, 0x80, 0x40

        };

        public static ushort GenerateCRC(byte[] data, int dataLength)
        {

            byte crcHi = 0xff;  // high crc byte initialized
            byte crcLo = 0xff;  // low crc byte initialized 

            for (int i = 0; i < dataLength; i++)
            {
                ushort crcIndex = (ushort)(crcHi ^ data[i]); // calculate the crc lookup index

                crcHi = (byte)(crcLo ^ crcLookupTableHi[crcIndex]);
                crcLo = crcLookupTableLo[crcIndex];
            }

            return (ushort)(crcHi << 8 | crcLo);

        }

        //private static object lockObject=new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ushort GenerateCRC(byte[] data, int startIndex, int length)
        {

            byte crcHi = 0xff;  // high crc byte initialized
            byte crcLo = 0xff;  // low crc byte initialized 

            for (int i = 0; i < length; i++)
            {
                ushort crcIndex = (ushort)(crcHi ^ data[(startIndex + i) % data.Length]); // calculate the crc lookup index

                crcHi = (byte)(crcLo ^ crcLookupTableHi[crcIndex]);
                crcLo = crcLookupTableLo[crcIndex];
            }

            return (ushort)(crcHi << 8 | crcLo);


            /*
            uint i, j;
            ushort crc = 0xFFFF;//set all 1

            int len = data.Length;
            if (len <= 0)
            {
                crc = 0;
            }
            else
            {
                //len--;
                for (i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (j = 0; j <= 7; j++)
                    {
                        if ((crc & 1) != 0)
                        {
                            crc = (ushort)((crc >> 1) ^ 0xA001);
                        }

                        else
                        {
                            crc = (ushort)(crc >> 1);
                        } //
                    }
                }
            }

            byte crcHi = (byte)((crc & 0xff00) >> 8);//高位置
            byte crcLo = (byte)(crc & 0x00ff); //低位置

            //crc = (ushort)(crcHi << 8);
            crc = (ushort)(crcHi << 8 | crcLo);
            return crc;*/
        }
    }

    public static class CRCGenerator
    {
        //CRC lookup table to avoid bit-shifting loops.
        private static ushort[] crcLookupTable = {
            0x00000, 0x01189, 0x02312, 0x0329B, 0x04624, 0x057AD, 0x06536, 0x074BF, 0x08C48, 0x09DC1, 0x0AF5A, 0x0BED3, 0x0CA6C, 0x0DBE5, 0x0E97E, 0x0F8F7, 0x01081, 0x00108, 0x03393, 0x0221A, 0x056A5, 0x0472C, 0x075B7, 0x0643E, 0x09CC9, 0x08D40, 0x0BFDB, 0x0AE52, 0x0DAED, 0x0CB64, 0x0F9FF, 0x0E876, 0x02102, 0x0308B, 0x00210, 0x01399, 0x06726, 0x076AF, 0x04434, 0x055BD, 0x0AD4A, 0x0BCC3, 0x08E58, 0x09FD1, 0x0EB6E, 0x0FAE7, 0x0C87C, 0x0D9F5, 0x03183, 0x0200A, 0x01291, 0x00318, 0x077A7, 0x0662E, 0x054B5, 0x0453C, 0x0BDCB, 0x0AC42, 0x09ED9, 0x08F50, 0x0FBEF, 0x0EA66, 0x0D8FD, 0x0C974, 0x04204, 0x0538D, 0x06116, 0x0709F, 0x00420, 0x015A9, 0x02732, 0x036BB, 0x0CE4C, 0x0DFC5, 0x0ED5E, 0x0FCD7, 0x08868, 0x099E1, 0x0AB7A, 0x0BAF3, 0x05285, 0x0430C, 0x07197, 0x0601E, 0x014A1, 0x00528, 0x037B3, 0x0263A, 0x0DECD, 0x0CF44, 0x0FDDF, 0x0EC56, 0x098E9, 0x08960, 0x0BBFB, 0x0AA72, 0x06306, 0x0728F, 0x04014, 0x0519D, 0x02522, 0x034AB, 0x00630, 0x017B9, 0x0EF4E, 0x0FEC7, 0x0CC5C, 0x0DDD5, 0x0A96A, 0x0B8E3, 0x08A78, 0x09BF1, 0x07387, 0x0620E, 0x05095, 0x0411C, 0x035A3, 0x0242A, 0x016B1, 0x00738, 0x0FFCF, 0x0EE46, 0x0DCDD, 0x0CD54, 0x0B9EB, 0x0A862, 0x09AF9, 0x08B70, 0x08408, 0x09581, 0x0A71A, 0x0B693, 0x0C22C, 0x0D3A5, 0x0E13E, 0x0F0B7, 0x00840, 0x019C9, 0x02B52, 0x03ADB, 0x04E64, 0x05FED, 0x06D76, 0x07CFF, 0x09489, 0x08500, 0x0B79B, 0x0A612, 0x0D2AD, 0x0C324, 0x0F1BF, 0x0E036, 0x018C1, 0x00948, 0x03BD3, 0x02A5A, 0x05EE5, 0x04F6C, 0x07DF7, 0x06C7E, 0x0A50A, 0x0B483, 0x08618, 0x09791, 0x0E32E, 0x0F2A7, 0x0C03C, 0x0D1B5, 0x02942, 0x038CB, 0x00A50, 0x01BD9, 0x06F66, 0x07EEF, 0x04C74, 0x05DFD, 0x0B58B, 0x0A402, 0x09699, 0x08710, 0x0F3AF, 0x0E226, 0x0D0BD, 0x0C134, 0x039C3, 0x0284A, 0x01AD1, 0x00B58, 0x07FE7, 0x06E6E, 0x05CF5, 0x04D7C, 0x0C60C, 0x0D785, 0x0E51E, 0x0F497, 0x08028, 0x091A1, 0x0A33A, 0x0B2B3, 0x04A44, 0x05BCD, 0x06956, 0x078DF, 0x00C60, 0x01DE9, 0x02F72, 0x03EFB, 0x0D68D, 0x0C704, 0x0F59F, 0x0E416, 0x090A9, 0x08120, 0x0B3BB, 0x0A232, 0x05AC5, 0x04B4C, 0x079D7, 0x0685E, 0x01CE1, 0x00D68, 0x03FF3, 0x02E7A, 0x0E70E, 0x0F687, 0x0C41C, 0x0D595, 0x0A12A, 0x0B0A3, 0x08238, 0x093B1, 0x06B46, 0x07ACF, 0x04854, 0x059DD, 0x02D62, 0x03CEB, 0x00E70, 0x01FF9, 0x0F78F, 0x0E606, 0x0D49D, 0x0C514, 0x0B1AB, 0x0A022, 0x092B9, 0x08330, 0x07BC7, 0x06A4E, 0x058D5, 0x0495C, 0x03DE3, 0x02C6A, 0x01EF1, 0x00F78
        };

        public static ushort GenerateCRC(byte[] data, int dataLength, ushort seed)
        {
            ushort newCrc;

            newCrc = seed;
            for (int i = 0; i < dataLength; i++)
            {
                newCrc = (ushort)((newCrc >> 8) ^ crcLookupTable[(newCrc ^ data[i]) & 0xff]);
            }

            return ((ushort)~newCrc);
        }

        public static ushort GenerateCRC(byte[] data, int startIndex, int length, ushort seed)
        {
            ushort newCrc;

            newCrc = seed;
            for (int i = 0; i < length; i++)
            {
                newCrc = (ushort)((newCrc >> 8) ^ crcLookupTable[(newCrc ^ data[(startIndex + i) % data.Length]) & 0xff]);
            }

            return ((ushort)~newCrc);
        }
    }
}

