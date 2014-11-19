using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace SerialPortController
{
    /// <summary>
    /// 设备工作模式
    /// </summary>
    /// <remarks>
    /// 0主动
    /// 1被动
    /// </remarks>
    public enum ReportWorkMode
    {
        /// <summary>
        /// 主动方式
        /// </summary>
        Initiative = 0,
        /// <summary>
        /// 被动方式
        /// </summary>
        Passive = 1
    }

    public interface ISerialListener : IDisposable
    {
        /*
        int WaitTime
        {
            get;
            set;
        }*/

        bool IsOpen
        {
            get;
        }

        ReportWorkMode ReportMode
        {
            get;
        }

        bool TimedOut
        {
            get;
            set;
        }

        void Close();

        byte[] CreateByteArrayOfLength(byte[] bArray, int length);

        byte[] Ping(UInt32 address, byte[] data);

        IDataFramePacket Send(UInt32 address, byte type/*命令头*/, byte dataLength/*数据长度*/, byte[] data/*数据内容*/);

        string CreateStringOfLength(string s, int length);

        void Start();

        void Stop();

        bool ChangeSetting(string portName, int baudRate, ReportWorkMode reportMode);

        bool ChangeWorkMode(SerialPortController.ReportWorkMode workMode);
    }

    

    public class ComOnEventArgs : EventArgs
    {
        public string PortName;
        public bool ComOn;
        public ComOnEventArgs(string portName, bool comOn)
        {
            this.PortName = portName;
            this.ComOn = comOn;
        }

        public override string ToString()
        {
            return this.PortName + (this.ComOn ? ":打开" : ":关闭");
        }
    }
}
