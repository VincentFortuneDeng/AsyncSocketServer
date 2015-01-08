using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTERIXDecode
{
    public class HDLCPacketTAG
    {
        /*
         *  UINT16 YC_HDLC_Flag;标志字段，必须为 0x5943
            UINT16 sequence;网络数据包流水号
            UINT32 deviceSnID;源设备序列编号，串口转网络时有效，
         *                    代表 HDLC-X 设备 SN 编号
            UINT8 serialNiD;串口编号，串口转网络时代表源串口号，
         *                  网络转串口时代表目的设备串口号：
         *                      HDLC-2800： 1～8
                                HDLC-121： 1～2
                                HDLC-125： 1～2
            UINT8 hdlc_with_fcs;  1： HDLC 帧携带 FCS 字段
                                  0： HDLC 帧无 FCS 字段
            UINT16 hdlc_frame_len;HDLC 帧长度（字节数）
            UINT32 tickCout;设备内部时钟 tick 值， 1 个 tick 约 10ms
            UINT32 sntp_time_sec;SNTP 秒时戳，保留，未来扩展
            UINT32 sntp_time_nsec;SNTP 纳秒时戳，保留，未来扩展
         * 
         */
        public UInt16 Sequence;
        public UInt32 DeviceSnID;
        public byte SerialNiD;
        public byte HDLC_With_FCS;
        public UInt16 HDLC_Frame_Len;
        public UInt32 TickCout;
        public UInt32 SNTP_Time_Sec;
        public UInt32 SNTP_Time_NSec;
    }
}
