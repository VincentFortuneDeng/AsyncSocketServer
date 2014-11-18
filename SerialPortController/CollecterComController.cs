using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace SerialPortController
{
    public class CollecterComController : SerialController
    {
        #region private const

        //private new const int MAX_PACKET_LENGTH = MAX_DATA_LENGTH + 4;

        #endregion private const

        #region private variables

        #endregion private variables

        #region public enum
        //public enum CursorStyle { NoCursor = 0, BlinkingBlockCursor = 1, UnderscoreCursor = 2, BlinkingBlockUnderscoreCursor = 3 }

        //[FlagsAttribute]
        //public enum Fans { Fan1 = 1, Fan2 = 2, Fan3 = 4, Fan4 = 8 }
        #endregion public enum

        #region  Constructor
        public CollecterComController(string portName, int baudRate, ReportWorkMode reportMode, bool discardNull)
            : base(portName, baudRate, reportMode, discardNull)
        {

        }
        #endregion  constructor



        #region Public SetAddress
        public override byte[] Ping(UInt32 address, byte[] data)
        {
            byte type = 0;

            if (16 < data.Length)
            {
                throw new ArgumentException("data", "must have 16 items or less");
            }

            //Trace.WriteLine("ping");
            return SendReturnData(address, type, (byte)data.Length, data);
        }
        /*
        public bool WriteLine1(string line)
        {
            byte type = 7;

            return WriteLine(type, line);
        }*/

        /*
        public bool WriteLine2(string line)
        {
            byte type = 8;
            return WriteLine(type, line);
        }*/

        //0-50(0=light 50= very dark)

        //0-100(0= 0ff 100=on)
        /*
        public bool SetupFanReporting(Fans fans)
        {
            byte type = 16;
            byte[] data = new byte[1];

            if ((int)fans < (int)Fans.Fan1 || (int)(Fans.Fan1 | Fans.Fan2 | Fans.Fan3 | Fans.Fan4) < (int)fans)
            {
                throw new ArgumentOutOfRangeException("fans", "fan value out of range");
            }

            data[0] = (byte)fans;
            return SendReturnBool(type, 1, data);
        }
        public bool SetFan1Power(int fan1)
        {
            return SetFanPower(fan1, fan2Power, fan3Power, fan4Power);
        }
        public bool SetFan2Power(int fan2)
        {
            return SetFanPower(fan1Power, fan2, fan3Power, fan4Power);
        }
        public bool SetFan3Power(int fan3)
        {
            return SetFanPower(fan1Power, fan2Power, fan3, fan4Power);
        }
        public bool SetFan4Power(int fan4)
        {
            return SetFanPower(fan1Power, fan2Power, fan3Power, fan4);
        }
        public bool SetFanPower(int fan1, int fan2, int fan3, int fan4)
        {
            byte type = 17;
            byte[] data = new byte[4];

            if (fan1 < 0 || 100 < fan1)
                throw new ArgumentOutOfRangeException("fan1", "must be 0-100");

            if (fan2 < 0 || 100 < fan2)
                throw new ArgumentOutOfRangeException("fan2", "must be 0-100");

            if (fan3 < 0 || 100 < fan3)
                throw new ArgumentOutOfRangeException("fan3", "must be 0-100");

            if (fan4 < 0 || 100 < fan4)
                throw new ArgumentOutOfRangeException("fan4", "must be 0-100");

            data[0] = fan1Power = (byte)fan1;
            data[1] = fan2Power = (byte)fan2;
            data[2] = fan3Power = (byte)fan3;
            data[3] = fan4Power = (byte)fan4;
            return SendReturnBool(type, 4, data);
        }*/
        public void Reset()
        {
            /*SetLCDBacklight(100);
            SetLCDContrast(15);
            SetFan1Power(0);
            WriteLine1("Crystalfontz 633");
            WriteLine2("HW v1.2  FW v1.0");*/
        }
        // Public IDisposable.Dispose implementation - calls the internal helper,


        // This is "friendly" wrapper method for IDisposable.Dispose

        #endregion Public SetAddress

        #region Private SetAddress

        #region REVIEW
        /*
        private bool WriteLine(byte type, string line)
        {
            line = CreateStringOfLength(line, LINE_LENGTH);
            return SendReturnBool(type, 16, System.Text.Encoding.ASCII.GetBytes(line.ToCharArray()));
        }*/
        /*
        private bool SetCursorStyle(CursorStyle cursorStyle)
        {
            byte type = 12;
            byte[] data = new byte[1];

            if (0 > (int)cursorStyle || 3 < (int)cursorStyle)
            {
                throw new ArgumentOutOfRangeException("cursorStyle", "must be a valid value in the enum");
            }

            data[0] = (byte)cursorStyle;
            return SendReturnBool(type, 1, data);
        }*/
        //创建指定长度字符串

        //发送数据后返回响应数据
        public override byte[] SendReturnData(UInt32 address, byte type, byte dataLength, byte[] data)
        {
            RtuPacket packet = (RtuPacket)Send(address, type, dataLength, data);

            if (null != packet)
            {
                return packet.Data;
            }

            else
            {
                return null;
            }
        }

        //发送数据返回布尔值
        public override bool SendReturnBool(UInt32 address, byte type, byte dataLength, byte[] data)
        {
            RtuPacket packet = (RtuPacket)Send(address, type, dataLength, data);

            if (null != packet)
            {
                return (type == (packet.Type & 0x1f)) && (DataPacketType.NORMAL_RESPONSE ==
                    responsePacket.PacketType && (byte)(address >> 16) == packet.Address);
            }

            else
            {
                return false;
            }
        }
        private const byte READ_VAULES = 0x03;
        private const byte WRITE_MULTI_VALUE = 0x10;
        private const byte WRITE_SINGLE_VALUE = 0x06;

        //添加数据包
        protected override bool AddPacket(byte[] buffer, int startIndex)
        {
            IDataFramePacket packet = CreatePacket(buffer, startIndex);//创建包

            //byte[] toCRCBytes = new byte[packet.Type == READ_VAULES ? packet.DataLength + 3 : 6];

            //Array.Copy(buffer, startIndex, toCRCBytes, 0, toCRCBytes.Length);
            //CRC16Generator crc16Generator = new SerialPortController.CRC16Generator();
            ushort calculatedCRC = CRC16Generator.GenerateCRC(buffer, startIndex, packet.Type == READ_VAULES ? packet.DataLength + 3 : 6);//重新计算CRC
            //ushort calculatedCRC = CRC16Generator.GenerateCRC(buffer, startIndex, packet.Type == READ_VAULES ? packet.DataLength + 3 : 6);//重新计算CRC

            if (calculatedCRC != packet.CRC)
            {
                //MessageBox.Show("CRC错误");
                //string outputString = "";
                //calculatedCRC = CRC16Generator.GenerateCRC(buffer, startIndex, packet.Type == READ_VAULES ? packet.DataLength + 3 : 6);//重新计算CRC
                /*foreach (byte bt in packet.Data)
                {
                    outputString += " " + Convert.ToString(bt, 16);
                }*/
                //Trace.WriteLine("Recevie " + outputString);
                Console.WriteLine("Recevie:" + "CRC ERROR!(CRC 错误): Calculated(计算值) CRC={0} Actual(实际值) CRC={1}",
                    Convert.ToString(calculatedCRC, 16), Convert.ToString(packet.CRC, 16));

                return false;
            }

            else //if (calculatedCRC == packet.CRC)
            {
                switch (packet.PacketType)
                {
                    case DataPacketType.NORMAL_RESPONSE://正常响应包
                        AddResponsePacket(packet);//添加响应包 并通知发送线程
                        break;

                    case DataPacketType.NORMAL_REPORT://正常报告包 
                        AddReportPacket(packet);//添加报告包 并通知报告线程
                        break;

                    case DataPacketType.SPECIAL_RESPONSE://错误响应包
                        AddResponsePacket(packet);//添加响应包 并通知发送线程
                        AddReportPacket(packet);//添加报告包 并通知报告线程
                        break;
                }
                return true;
            }
        }

        //添加数据包
        /*
        private  bool AddPacketWithCRC(byte[] buffer, int startIndex)
        {
            FramePacket packet = CreatePacketWithCRC(buffer, startIndex);//创建包
            ushort calculatedCRC = CRCGenerator.GenerateCRC(buffer, startIndex, packet.DataLength + 2, CRC_SEED);//重新计算CRC

            switch (packet.PacketType)
            {
                case CommunicationPacketType.NORMAL_RESPONSE://正常响应包
                    AddResponsePacket(packet);//添加响应包 并通知发送线程
                    break;

                case CommunicationPacketType.NORMAL_REPORT://正常报告包 
                    AddReportPacket(packet);//添加报告包 并通知报告线程
                    break;

                case CommunicationPacketType.ERROR_RESPONSE://错误响应包
                    AddResponsePacket(packet);//添加响应包 并通知发送线程
                    break;
            }

            if (calculatedCRC != packet.CRC)
            {
                Console.WriteLine("CRC ERROR!(CRC 错误)!!: Calculated(计算值) CRC={0} Actual(实际值) CRC={1}",
                    Convert.ToString(calculatedCRC, 16), packet.CRC);

                return false;
            }

            return true;
        }*/
        //添加响应包


        //添加报告包


        //添加响应包


        //添加报告包


        //报告键盘激活句柄//关闭线程

        /*
        * This class wraps a SerialPort instance that is not exposed 
        * directly to the consuming application.  The SerialPort implements 
        * IDisposable, and calling Dispose there causes the SafeHandle
        * given by the OS to the serial port to be released. 
        * 
        * 'disposing' is true if Close or Dispose have been called explicitly,
        * at a point when all managed references are still valid. 
        * Since this class does not directly refer to any system resources, and
        * managed resources can only be cleaned up when the refs are valid, we
        * only clean-up if explicitly called.
        * 
        * Proper usage of this class is to call Close() as soon as the port isn't 
        * needed anymore (as in MainForm.cs). However, if Dispose isn't called here, 
        * then the automatic Dispose call from finalization of the SerialPort 
        * itself will free the OS handle to the serial port.
        * 
        * Note that we would also want to call GCSuppressFinalize (regardless of the
        * value of disposing) if this class were finalizable (i.e. if it had a finalizer).
        * In this case, there are no direct refs to an unmanaged resource, so custom finalization
        * code isn't necessary.
        */

        //释放系统资源
        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing && com != null && com.IsOpen)
            {
                Reset();
                com.Close();
                CloseThreads();

                // Keep us from calling resetting or closing multiple times
                disposed = true;
            }
        }

        //二进制字节数据转换为包数据
        protected override IDataFramePacket CreatePacket(byte[] buffer, int startIndex)
        {
            byte type = buffer[startIndex + 1];
            byte dataLength = 0;
            ushort crc = 0;
            byte[] data = new byte[0];

            switch (type)
            {
                case READ_VAULES:
                    dataLength = buffer[(startIndex + 2) % buffer.Length];
                    data = new byte[dataLength];

                    for (int i = 0; i < dataLength; i++)//缓冲区为循环设计
                    {
                        data[i] = buffer[(startIndex + 3 + i) % buffer.Length];
                    }

                    crc |= (ushort)(buffer[(startIndex + 3 + dataLength) % buffer.Length] << 8);
                    crc |= (ushort)buffer[(startIndex + 3 + dataLength + 1) % buffer.Length];

                    break;

                case WRITE_SINGLE_VALUE:
                    //dataLength = 0;
                    //data = new byte[dataLength];

                    crc |= (ushort)(buffer[6] << 8);
                    crc |= (ushort)buffer[7];

                    break;

                case WRITE_MULTI_VALUE:
                    //dataLength = 0;
                    //data = new byte[dataLength];
                    /*
                    for (int i = 0; i < dataLength; i++)//缓冲区为循环设计
                    {
                        data[i] = buffer[(startIndex + 3 + i) % buffer.Length];
                    }*/

                    crc |= (ushort)(buffer[(startIndex + 6 + dataLength) % buffer.Length] << 8);
                    crc |= (ushort)buffer[(startIndex + 6 + dataLength + 1) % buffer.Length];

                    break;

                default:

                    break;
            }

            return new RtuPacket((buffer[startIndex]), type, dataLength, data, crc);
        }
        #endregion REVIEW

        //发送数据


        //发送数据


        public override IDataFramePacket Send(UInt32 address, byte type/*命令头*/, byte dataLength/*数据长度*/, byte[] data/*数据内容*/)
        {
            ushort crc = 0x0000;
            //string outputstr = "";
            //CRC16Generator crc16Generator = new CRC16Generator();
            switch (type)
            {
                case READ_VAULES:
                    if (dataLength == 0)
                    {
                        throw new ArgumentException("发送数据无效");
                    }
                    //XMit transmit 传输，转送，传达，传导，发射，遗传，传播，发射信号(代号)
                    packetXMitBuffer[0] = (byte)(address >> 16);//设备地址
                    packetXMitBuffer[1] = type;//命令头 
                    packetXMitBuffer[2] = (byte)(address >> 8);//数据地址
                    packetXMitBuffer[3] = (byte)(address);//数据地址
                    packetXMitBuffer[4] = 0x00;
                    packetXMitBuffer[5] = (byte)(dataLength / 2);//数据长度

                    crc = CRC16Generator.GenerateCRC(packetXMitBuffer, 0, 6);//生成CRC校验码
                    packetXMitBuffer[6] = (byte)(crc >> 8);//高8位
                    packetXMitBuffer[7] = (byte)crc;//低8位

                    lock (responseSignal)
                    {
                        responsePacket = null;//清空响应包
                        // comPort

                        /*string outputstr = "";

                        for (int i = 0; i < 8; i++)
                        {
                            outputstr += Convert.ToString(packetXMitBuffer[i], 16)+" ";
                        }*/

                        //Trace.WriteLine("send8 " + outputstr);
                        com.Write(packetXMitBuffer, 0, 8);
                        //MessageBox.Show(outputstr);
                        //DateTime sendTime = DateTime.Now;
                        if (Monitor.Wait(responseSignal, (int)(MAX_RESPONSE_TIME + (dataLength + 5) * 8L / (packetXMitBuffer[0] == 3 ? 19200 : 4800) * 1000)))//发送后等待响应
                        {
                            //MessageBox.Show("Receive");
                            //Trace.WriteLine(DateTime.Now - sendTime);
                            return responsePacket;
                        }
                    }

                    break;

                case WRITE_MULTI_VALUE:
                    if ((data == null && dataLength != 0) || dataLength > data.Length)
                    {
                        throw new ArgumentException("发送数据无效");
                    }

                    //XMit transmit 传输，转送，传达，传导，发射，遗传，传播，发射信号(代号)
                    packetXMitBuffer[0] = (byte)(address >> 16);//设备地址
                    packetXMitBuffer[1] = type;//命令头 
                    packetXMitBuffer[2] = (byte)(address >> 8);//数据地址
                    packetXMitBuffer[3] = (byte)(address);//数据地址
                    packetXMitBuffer[4] = 0x00;
                    packetXMitBuffer[5] = (byte)(dataLength / 2);//数据长度
                    packetXMitBuffer[6] = dataLength;//数据长度
                    if (0 != dataLength)
                    {
                        Array.Copy(data, 0, packetXMitBuffer, 7, dataLength);
                    }
                    crc = CRC16Generator.GenerateCRC(packetXMitBuffer, 0, dataLength + 7);//生成CRC校验码
                    packetXMitBuffer[7 + dataLength] = (byte)(crc >> 8);//高8位
                    packetXMitBuffer[7 + dataLength + 1] = (byte)crc;//低8位

                    lock (responseSignal)
                    {
                        responsePacket = null;//清空响应包
                        // comPort
                        /*string outputstr = "";

                        for (int i = 0; i < dataLength + 9; i++)
                        {
                            outputstr += Convert.ToString(packetXMitBuffer[i], 16) + " ";
                        }*/
                        //Trace.WriteLine((dataLength + 9) + "send " + outputstr);
                        com.Write(packetXMitBuffer, 0, dataLength + 9);

                        //MessageBox.Show(outputstr);
                        //DateTime sendTime = DateTime.Now;
                        if (Monitor.Wait(responseSignal, (int)(MAX_RESPONSE_TIME + 8 * 8L / (packetXMitBuffer[0] == 3 ? 19200 : 4800) * 1000)))//发送后等待响应
                        {
                            //Trace.WriteLine(DateTime.Now - sendTime);
                            //MessageBox.Show("Receive");
                            return responsePacket;
                        }
                    }

                    break;

                case WRITE_SINGLE_VALUE:
                    if ((data == null && dataLength != 0) || dataLength > data.Length)
                    {
                        throw new ArgumentException("发送数据无效");
                    }
                    //XMit transmit 传输，转送，传达，传导，发射，遗传，传播，发射信号(代号)
                    packetXMitBuffer[0] = (byte)(address >> 16);//设备地址
                    packetXMitBuffer[1] = type;//命令头
                    packetXMitBuffer[2] = (byte)(address >> 8);//数据地址
                    packetXMitBuffer[3] = (byte)address;//数据地址
                    if (0 != dataLength)
                    {
                        Array.Copy(data, 0, packetXMitBuffer, 4, dataLength);
                    }
                    crc = CRC16Generator.GenerateCRC(packetXMitBuffer, 0, 6);//生成CRC校验码
                    packetXMitBuffer[4 + dataLength] = (byte)(crc >> 8);//高8位
                    packetXMitBuffer[4 + dataLength + 1] = (byte)crc;//低8位

                    lock (responseSignal)
                    {
                        responsePacket = null;//清空响应包
                        // comPort
                        /*string outputstr = "";

                        for (int i = 0; i < 8; i++)
                        {
                            outputstr += Convert.ToString(packetXMitBuffer[i], 16) + " ";
                        }*/
                        //Trace.WriteLine("send8 " + outputstr);
                        com.Write(packetXMitBuffer, 0, 8);

                        //MessageBox.Show(outputstr);
                        //DateTime sendTime = DateTime.Now;
                        if (Monitor.Wait(responseSignal, (int)(MAX_RESPONSE_TIME + 8 * 8L / (packetXMitBuffer[0] == 3 ? 19200 : 4800) * 1000)))//发送后等待响应
                        {
                            //Trace.WriteLine(DateTime.Now - sendTime);
                            //MessageBox.Show("Receive");
                            return responsePacket;
                        }
                    }

                    break;

                default:

                    break;
            }

            RaiseOfflineEvent(new OnlineEventArgs((byte)(address >> 16), false));
            return null;//响应超时
        }

        //接收数据线程
        /*
        private override void Receive2()
        {
            try
            {
                byte[] receiveBuffer = new byte[128];//接收缓冲区
                int bytesRead = 0;//已读字节
                int bufferIndex = 0;//缓冲区索引
                int startPacketIndex = 0;//包开始索引
                int expectedPacketLength = -1;//期望包长度
                bool expectedPacketLengthIsSet = false;//期望包长度是否设置
                int numBytesToRead = receiveBuffer.Length;//要读取的字节数量

                //消息循环
                while (true)
                {
                    //因为该Demo协议 包长度在第二位 所以在期望包长度没有设置的情况下 bytesRead<=1也是继续读取的条件
                    if (expectedPacketLengthIsSet || 1 >= bytesRead)
                    {
                        //If the expectedPacketLength has been or no bytes have been read
                        //翻译:如果预期包长度已经设置或者没有字节已经被读取
                        //This covers the case that more then 1 entire packet has been read in at a time
                        //翻译:这包含一次读取超过一个完整包的情况
                        // comPort
                        try
                        {
                            bytesRead += com.Read(receiveBuffer, bufferIndex, numBytesToRead);//读取字节数据
                            bufferIndex = startPacketIndex + bytesRead;//修改缓冲区下一位置指针
                        }

                        catch (TimeoutException)
                        {
                            timedOut = true;//超时设置
                        }
                    }

                    if (1 < bytesRead)//长度超过1说明期望包长度字节应该出现(在第二位)
                    {
                        //The buffer has the singleRecordLength for the packet
                        //翻译:缓冲区已经存在包数据长度
                        if (!expectedPacketLengthIsSet)//设置包长度
                        {
                            //If the expectedPacketLength has not been set for this packet
                            //如果期望包长度没有设置
                            expectedPacketLength = receiveBuffer[(1 + startPacketIndex) % receiveBuffer.Length] + 4;//4字节为附加字节 CRC2 命令1 长度1
                            expectedPacketLengthIsSet = true;//长度已经设置
                        }

                        if (bytesRead >= expectedPacketLength)//已经有至少一个包长度的字节数据
                        {
                            //The buffer has atleast as many bytes for this packet
                            //翻译:缓冲区已经存在至少一个包或者多于一个包的字节数据
                            AddPacketWithCRC(receiveBuffer, startPacketIndex);//数据添加到包
                            expectedPacketLengthIsSet = false;//期望包长度重置(已经收集到一个包数据)
                            if (bytesRead == expectedPacketLength)//正好收集到一个包数据
                            {
                                //The buffer contains only the bytes for this packet
                                //翻译:缓冲区包含仅一个包的字节数据
                                bytesRead = 0;//重置接收字节数据计数器
                                bufferIndex = startPacketIndex;//缓冲区下一索引位置与包开始的位置相同
                            }
                            //缓冲区字节数据多于1个包的字节数据
                            else
                            {
                                //The buffer also has bytes for the next packet
                                //翻译:缓冲区包含下一数据包的字节数据
                                startPacketIndex += expectedPacketLength;//下一包的开始位置
                                startPacketIndex %= receiveBuffer.Length;//缓冲区溢出循环
                                bytesRead -= expectedPacketLength;//记录下下一包以读取数据字节数
                                bufferIndex = startPacketIndex + bytesRead;//缓冲区位置计数
                            }
                        }
                    }

                    bufferIndex %= receiveBuffer.Length;//缓冲区溢出循环
                    numBytesToRead = bufferIndex < startPacketIndex ? startPacketIndex - bufferIndex : receiveBuffer.Length - bufferIndex;//控制缓冲区循环使用时读取字节数
                }
            }

            catch (IOException)
            {
                // abort the thread
                //翻译:终止线程
                System.Threading.Thread.CurrentThread.Abort();
            }

            catch (ObjectDisposedException)
            {
                if (receiveThread != null)
                {
                    receiveThread = null;
                }
            }
        }*/

        //接收数据线程
        protected override void Receive()
        {
            try
            {
                byte[] receiveBuffer = new byte[1024];//接收缓冲区
                int bytesRead = 0;//已读字节
                int bufferIndex = 0;//缓冲区索引
                int startPacketIndex = 0;//包开始索引
                int expectedPacketLength = -1;//期望包长度
                bool expectedPacketLengthIsSet = false;//期望包长度是否设置
                int numBytesToRead = receiveBuffer.Length;//要读取的字节数量

                //消息循环
                while (true)
                {
                    //MessageBox.Show("Run"+workThreadRun); 
                    if (workThreadRun)
                    {
                        //MessageBox.Show(string.Format("expectedPacketLengthIsSet{0},bytesRead{1}", expectedPacketLengthIsSet, bytesRead));
                        //因为该Demo协议 包长度在第二位 所以在期望包长度没有设置的情况下 bytesRead<=1也是继续读取的条件
                        if (expectedPacketLengthIsSet || 3 > bytesRead)
                        {
                            //If the expectedPacketLength has been or no bytes have been read
                            //翻译:如果预期包长度已经设置或者没有字节已经被读取
                            //This covers the case that more then 1 entire packet has been read in at a time
                            //翻译:这包含一次读取超过一个完整包的情况
                            // comPort

                            try
                            {
                                //MessageBox.Show(bytesRead.ToString());
                                bytesRead += com.Read(receiveBuffer, bufferIndex, numBytesToRead);//读取字节数据
                                //Trace.WriteLine("bytesRead" + bytesRead);
                                bufferIndex = startPacketIndex + bytesRead;//修改缓冲区下一位置指针
                                //MessageBox.Show(bytesRead.ToString());
                            }

                            catch (TimeoutException)
                            {
                                timedOut = true;//超时设置
                            }
                        }

                        if (3 <= bytesRead)//长度超过1说明期望包长度字节应该出现(在第二位)
                        {
                            //The buffer has the dataLength for the packet
                            //翻译:缓冲区已经存在包数据长度
                            if (!expectedPacketLengthIsSet)//设置包长度
                            {
                                //If the expectedPacketLength has not been set for this packet
                                //如果期望包长度没有设置
                                int lenData = (receiveBuffer[(1 + startPacketIndex) % receiveBuffer.Length] == READ_VAULES ?
                                    receiveBuffer[(2 + startPacketIndex) % receiveBuffer.Length] + 5 : 8);
                                byte packetType = receiveBuffer[(1 + startPacketIndex) % receiveBuffer.Length];
                                byte address = receiveBuffer[startPacketIndex % receiveBuffer.Length];
                                //加载地址集合、包类型集合
                                if ((packetType == READ_VAULES || packetType == WRITE_MULTI_VALUE || packetType == WRITE_SINGLE_VALUE) &&
                                    (address == 1 || address == 2 || address == 3 || address == 0) && lenData <= MAX_PACKET_LENGTH)
                                {
                                    expectedPacketLength = lenData;//4字节为附加字节 CRC2 命令1 长度1
                                    expectedPacketLengthIsSet = true;//长度已经设置
                                }

                                else
                                {
                                    expectedPacketLengthIsSet = false;//期望包长度重置(已经收集到一个包数据)
                                    //The buffer contains only the bytes for this packet
                                    //翻译:缓冲区包含仅一个包的字节数据
                                    bytesRead = 0;//重置接收字节数据计数器
                                    bufferIndex = startPacketIndex = 0;//缓冲区下一索引位置与包开始的位置相同
                                }
                            }

                            if (bytesRead >= expectedPacketLength)//已经有至少一个包长度的字节数据
                            {
                                //The buffer has atleast as many bytes for this packet
                                //翻译:缓冲区已经存在至少一个包或者多于一个包的字节数据
                                bool added = AddPacket(receiveBuffer, startPacketIndex);
                                //数据添加到包
                                expectedPacketLengthIsSet = false;//期望包长度重置(已经收集到一个包数据)
                                if (bytesRead == expectedPacketLength || !added)//正好收集到一个包数据
                                {
                                    //The buffer contains only the bytes for this packet
                                    //翻译:缓冲区包含仅一个包的字节数据
                                    bytesRead = 0;//重置接收字节数据计数器
                                    bufferIndex = startPacketIndex = 0;//缓冲区下一索引位置与包开始的位置相同
                                }
                                //缓冲区字节数据多于1个包的字节数据
                                else
                                {
                                    //The buffer also has bytes for the next packet
                                    //翻译:缓冲区包含下一数据包的字节数据
                                    startPacketIndex += expectedPacketLength;//下一包的开始位置
                                    startPacketIndex %= receiveBuffer.Length;//缓冲区溢出循环
                                    bytesRead -= expectedPacketLength;//记录下下一包以读取数据字节数
                                    bufferIndex = startPacketIndex + bytesRead;//缓冲区位置计数
                                }
                            }
                        }

                        bufferIndex %= receiveBuffer.Length;//缓冲区溢出循环
                        numBytesToRead = bufferIndex < startPacketIndex ? startPacketIndex - bufferIndex : receiveBuffer.Length - bufferIndex;//控制缓冲区循环使用时读取字节数
                    }

                    Thread.Sleep(1);
                }
            }

            catch (IOException ioe)
            {
                // abort the thread
                //翻译:终止线程
                //System.Threading.Thread.CurrentThread.Abort();
                throw ioe;
            }

            catch (ObjectDisposedException ode)
            {
                if (receiveThread != null)
                {
                    receiveThread = null;
                }
                throw ode;
            }
        }

        //事件报告线程
        protected override void ReportEventHandler()
        {
            try
            {
                //报告包引用变量
                IDataFramePacket packet = null;

                //线程循环
                while (true)
                {
                    if (this.eventThreadRun)
                    {
                        //等待接收线程的报告通知
                        while (null == packet)
                        {
                            lock (reportSignal)
                            {
                                //如果报告队列中已经存在报告 则取出报告包进行处理
                                if (0 != reportQueue.Count)
                                {
                                    packet = reportQueue.Dequeue();
                                }

                                //如果没有报告则等待报告(线程等待)
                                else
                                {
                                    Monitor.Wait(reportSignal);
                                }
                            }
                        }//end 等待接收线程的报告通知

                        //处理报告包
                        //switch (packet.Type)
                        //{

                        //}
                        packet = null;
                    }

                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
            }

            catch (IOException ioe)
            {
                // abort the thread
                //System.Threading.Thread.CurrentThread.Abort();
                throw ioe;
            }

            catch (ObjectDisposedException ode)
            {
                if (eventThread != null)
                {
                    eventThread = null;
                }
                throw ode;
            }
        }
        #endregion Private SetAddress
    }
}
