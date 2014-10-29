using System;
using System.Net.Sockets;

namespace AsyncSockets.AsyncSocketClient
{
    class DSCClientDisconnectedEventArgs : EventArgs
    {        
        public Socket socket;

        public DSCClientDisconnectedEventArgs(Socket soc)
        {            
            this.socket = soc;
        }
    }
}
