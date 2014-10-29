using System;
using System.Net.Sockets;

namespace AsyncSockets.AsyncSocketClient
{
    public class DSCClientConnectedEventArgs:EventArgs
    {        
        public Socket socket;

        public DSCClientConnectedEventArgs(Socket soc)
        {            
            this.socket = soc;
        }
    }
}
