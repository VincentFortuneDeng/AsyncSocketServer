using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;

namespace SerialPortListener
{
    public class DADListener : SerialListener
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
        public DADListener(string portName, int baudRate, ReportWorkMode reportMode, bool discardNull)
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
            IDataFramePacket packet = Send(address, type, dataLength, data);

            if (null != packet)
            {
                return Encoding.ASCII.GetBytes(packet.GetDataStrings()[0]);
            }
            else
            {
                return null;
            }
        }

        //发送数据后返回响应数据
        public DistanceData SendReturnData(byte address, byte type, string data)
        {
            DistancePacket packet = (DistancePacket)Send(address, type, data);

            if (null != packet)
            {
                DistanceData recordData = GenerateRecordData(packet);

                return recordData;
            }
            else
            {
                return null;
            }
        }

        public bool SendReturnBool(byte address, byte type, string data)
        {
            DistancePacket packet = (DistancePacket)Send(address, type, data);

            if (null != packet)
            {
                return (type == packet.Type % 0xA
                    && DataPacketType.NORMAL_RESPONSE == responsePacket.PacketType
                    && packet.BoolData && address == packet.Address);
            }

            else
            {
                return false;
            }
        }

        //发送数据返回布尔值
        public override bool SendReturnBool(UInt32 address, byte type, byte dataLength, byte[] data)
        {
            IDataFramePacket packet = Send(address, type, dataLength, data);

            if (null != packet)
            {
                return (type == (packet.Type & 0x0f) && DataPacketType.NORMAL_RESPONSE == responsePacket.PacketType);
            }

            else
            {
                return false;
            }
        }

        private bool AddPacket(string stringPacket)
        {
            string[] pakcetFields = stringPacket.Split('.');

            IDataFramePacket packet = CreatePacket(stringPacket);//创建包
            //ushort calculatedCRC = CRC16Generator.GenerateCRC(buffer, startIndex, packet.Type == READ_VAULES ? packet.DataLength + 3 : 6);//重新计算CRC
            //if (calculatedCRC == packet.CRC)
            //{
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
            //}
            /*
           if (calculatedCRC != packet.CRC)
           {
               string outputString = "";
               foreach (byte bt in packet.Data)
               {
                   outputString += Convert.ToString(bt, 16);
               }
               //Trace.WriteLine("Recevie " + outputString);
               Console.WriteLine("Recevie " + outputString + "CRC ERROR!(CRC 错误): Calculated(计算值) CRC={0} Actual(实际值) CRC={1}",
                   Convert.ToString(calculatedCRC, 16), packet.CRC);

               return false;
           }*/

            return true;

        }

        //添加数据包
        protected override bool AddPacket(byte[] buffer, int startIndex)
        {
            IDataFramePacket packet = CreatePacket(buffer, startIndex);//创建包

            ushort calculatedCRC = CRCGenerator.GenerateCRC(buffer, startIndex, packet.DataLength + 2, CRC_SEED);//重新计算CRC

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

            /*
            if (calculatedCRC != packet.CRC)
            {
                Console.WriteLine("CRC ERROR!(CRC 错误): Calculated(计算值) CRC={0} Actual(实际值) CRC={1}",
                    Convert.ToString(calculatedCRC, 16), packet.CRC);

                return false;
            }*/

            return true;
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

        //private const byte REDA_VALUES = 0x03;
        //private const byte WRITE_MULTI_VAULE = 0x10;
        //private const byte WRITE_SINGLE_VALUE = 0x06;

        protected IDataFramePacket CreatePacket(string stringPacket)
        {
            string[] packetFields = stringPacket.Split('.');

            byte address = Convert.ToByte(packetFields[1]);

            byte type = Convert.ToByte(packetFields[0], 16);


            if (packetFields.Length == 3)
            {
                bool boolData = packetFields[2] == "1" ? true : false;

                return new DistancePacket(address, type, boolData);
            }

            else
            {
                if (this.reportMode == ReportWorkMode.Initiative)
                {
                    type |= 0xf0;
                }

                string stringData = string.Join("|", packetFields, 2, packetFields.Length - 2);

                return new DistancePacket(address, type, stringData);
            }
        }

        //二进制字节数据转换为包数据
        protected override IDataFramePacket CreatePacket(byte[] buffer, int startIndex)
        {/*
            byte type = buffer[startIndex + 1];

            byte dataLength = (type == REDA_VALUES ?
                buffer[(startIndex + 2) % buffer.Length] :
                buffer[(startIndex + 3) % buffer.Length]);//缓冲区为循环设计

            byte[] data = (type == REDA_VALUES ? new byte[dataLength] : new byte[1]);
            ushort crc = 0;
            if (type != REDA_VALUES)
            {
                for (int i = 0; i < dataLength; i++)//缓冲区为循环设计
                {
                    data[i] = buffer[(startIndex + 3 + i) % buffer.Length];
                }
            }

            crc |= (ushort)(buffer[(startIndex + 3 + dataLength) % buffer.Length] << 8);
            crc |= (ushort)buffer[(startIndex + 3 + dataLength + 1) % buffer.Length];

            return new RtuPacket((byte)(buffer[startIndex] >> 16), type, dataLength, data, crc);
          * */
            throw new NotImplementedException();
        }
        #endregion REVIEW

        //发送数据
        private const string SEPARATOR = ".";

        //发送数据
        private IDataFramePacket Send(byte address, byte type/*命令头*/, string data/*数据内容*/)
        {
            string stringPacketXMitBuffer = FRAME_HEAD + Convert.ToString(type, 10);
            stringPacketXMitBuffer += SEPARATOR + Convert.ToString(address, 10);
            if (!string.IsNullOrEmpty(data))
            {
                stringPacketXMitBuffer += SEPARATOR + data;
            }
            stringPacketXMitBuffer += FRAME_TAIL;
            //using (StreamWriter sw = new StreamWriter(@".\" + address + ".log", true, Encoding.ASCII))
            //{
            //    sw.WriteLine(stringPacketXMitBuffer);
            //    sw.Close();
            //}
            
            //ushort crc;
            /*
            if ((null == data && dataLength != 0) || dataLength > data.Length)
            {
                throw new ArgumentException("发送数据无效");
            }*/
            //XMit transmit 传输，转送，传达，传导，发射，遗传，传播，发射信号(代号)
            /*
            packetXMitBuffer[0] = type;//命令头 
            packetXMitBuffer[1] = dataLength;//数据长度*/
            /*if (0 != dataLength)
            {
                Array.Copy(data, 0, packetXMitBuffer, 2, dataLength);
            }*/

            //crc = CRCGenerator.GenerateCRC(packetXMitBuffer, dataLength + 2, CRC_SEED);//生成CRC校验码
            //packetXMitBuffer[2 + dataLength + 1] = (byte)(crc >> 8);//高8位
            //packetXMitBuffer[2 + dataLength] = (byte)crc;//低8位

            lock (responseSignal)
            {
                responsePacket = null;//清空响应包
                // comPort

                com.Write(stringPacketXMitBuffer);

                //DateTime sendTime = DateTime.Now;

                if (Monitor.Wait(responseSignal, MAX_RESPONSE_TIME))//发送后等待响应
                {
                    //Trace.WriteLine(DateTime.Now - sendTime);
                    //using (StreamWriter sw = new StreamWriter(@".\" + address + ".log", true, Encoding.ASCII))
                    //{
                    //    sw.WriteLine(DateTime.Now - sendTime);
                    //    sw.Close();
                    //}
                    return responsePacket;
                }
            }

            RaiseOfflineEvent(new OnlineEventArgs(address, false));
            return null;//响应超时
        }

        private IDataFramePacket Send(ushort address, byte type/*命令头*/, byte dataLength/*数据长度*/, byte[] data/*数据内容*/)
        {
            //ushort crc;

            if ((null == data && dataLength != 0) || dataLength > data.Length)
            {
                throw new ArgumentException("发送数据无效");
            }
            //XMit transmit 传输，转送，传达，传导，发射，遗传，传播，发射信号(代号)

            packetXMitBuffer[0] = type;//命令头 
            packetXMitBuffer[1] = dataLength;//数据长度
            if (0 != dataLength)
            {
                Array.Copy(data, 0, packetXMitBuffer, 2, dataLength);
            }

            //crc = CRCGenerator.GenerateCRC(packetXMitBuffer, dataLength + 2, CRC_SEED);//生成CRC校验码
            //packetXMitBuffer[2 + dataLength + 1] = (byte)(crc >> 8);//高8位
            //packetXMitBuffer[2 + dataLength] = (byte)crc;//低8位

            lock (responseSignal)
            {
                responsePacket = null;//清空响应包
                // comPort

                com.Write(packetXMitBuffer, 0, dataLength + 4);
                if (Monitor.Wait(responseSignal, MAX_RESPONSE_TIME))//发送后等待响应
                {

                    return responsePacket;
                }
            }

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
                string receiveBuffer = string.Empty;
                string stringPacket = string.Empty;
                int startPacketIndex = -1;//当前包开始索引
                int nextStartPacketIndex = -1;
                int endPacketIndex = -1;
                bool packetFrameHeaderIsSet = false;//包开始符号是否设置

                //线程循环
                while (true)
                {
                    if (workThreadRun)
                    {
                        //因为该Demo协议 包长度在第二位 所以在期望包长度没有设置的情况下 bytesRead<=1也是继续读取的条件
                        if (packetFrameHeaderIsSet || 1 > receiveBuffer.Length)//复制存储缓冲区数据
                        {
                            //If the expectedPacketLength has been or no bytes have been read
                            //翻译:如果预期包长度已经设置或者没有字节已经被读取
                            //This covers the case that more then 1 entire packet has been read in at a time
                            //翻译:这包含一次读取超过一个完整包的情况
                            // comPort
                            try
                            {
                                receiveBuffer += com.ReadExisting();//读取字节数据//bufferIndex = startPacketIndex + bytesRead;//修改缓冲区下一位置指针
                            }

                            catch (TimeoutException)
                            {
                                timedOut = true;//超时设置
                            }
                        }

                        if (1 <= receiveBuffer.Length)//长度超过1说明期望包长度字节应该出现(在第二位) 处理已接收缓冲区数据
                        {
                            //The buffer has the singleRecordLength for the packet
                            //翻译:缓冲区已经存在包数据长度
                            if (!packetFrameHeaderIsSet)//设置帧开始标志
                            {
                                startPacketIndex = receiveBuffer.IndexOf(FRAME_HEAD);

                                packetFrameHeaderIsSet = startPacketIndex != -1;
                                if (!packetFrameHeaderIsSet)
                                {
                                    receiveBuffer = string.Empty;
                                }
                            }

                            if (packetFrameHeaderIsSet)//包开始符已经设置
                            {
                                endPacketIndex = receiveBuffer.IndexOf(FRAME_TAIL);
                                if (endPacketIndex != -1)
                                {
                                    stringPacket = receiveBuffer.Substring(startPacketIndex, endPacketIndex - startPacketIndex + 1);
                                    receiveBuffer = receiveBuffer.Remove(startPacketIndex, endPacketIndex - startPacketIndex + 1);

                                    AddPacket(stringPacket.Substring(1, stringPacket.Length - 2));

                                    packetFrameHeaderIsSet = false;
                                    nextStartPacketIndex = receiveBuffer.IndexOf(FRAME_HEAD);
                                    if (!string.IsNullOrEmpty(receiveBuffer))
                                    {
                                        if (nextStartPacketIndex > 0)
                                        {
                                            receiveBuffer = receiveBuffer.Remove(0, nextStartPacketIndex);
                                        }

                                        else if (nextStartPacketIndex == -1)
                                        {
                                            receiveBuffer = string.Empty;
                                        }
                                    }
                                }
                            }

                        }
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
                DistancePacket packet = null;

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
                                    packet = (DistancePacket)reportQueue.Dequeue();
                                }

                                //如果没有报告则等待报告(线程等待)
                                else
                                {
                                    Monitor.Wait(reportSignal);
                                }
                            }
                        }//end 等待接收线程的报告通知

                        DistanceData recordData = GenerateRecordData(packet);

                        if (recordData != null)
                        {
                            RaiseReportEvent(recordData);
                        }

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
        private DistanceData GenerateRecordData(DistancePacket packet)
        {
            if (null != packet)
            {
                try
                {
                    string[] dataFields = packet.StringData.Split('|');

                    DistanceData distanceData = new DistanceData();
                    distanceData.DataTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    distanceData.Address = packet.Address;
                    distanceData.Sign = dataFields[0].Substring(0, 1);
                    distanceData.Distance = Convert.ToInt32(dataFields[0].Substring(1), 10);
                    distanceData.Flameouted = dataFields[2].Contains("0");
                    distanceData.Battery = Convert.ToInt32(dataFields[1], 10);

                    return distanceData;
                }

                catch
                {
                    return null;
                }
            }

            else
            {
                return null;
            }
        }

        #endregion Private SetAddress
    }
}
