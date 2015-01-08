using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTERIXDecode
{
    public class HDLCPacket
    {
        public HDLCPacketTAG PacketTAG { get; set; }
        public byte Address { get; set; }
        public byte CF { get; set; }
        public byte[] Information { get; set; }
        public Int16 FCS { get; set; }
    }
}
