using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortController
{
    public class ResponseEventArgs:EventArgs
    {

        /// <summary>
        /// 结果
        /// </summary>
        private bool Result;
        /// <summary>
        /// 包命令
        /// </summary>
        private PacketCommandType PacketCommand;

        /// <param name="result">结果</param>
        /// <param name="packetCommand">包命令类型</param>
        public ResponseEventArgs(bool result, PacketCommandType packetCommand)
        {
            this.Result = result;
            this.PacketCommand = packetCommand;
        }
    }
}
