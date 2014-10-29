using System;
using System.Net.Sockets;
namespace AsyncSockets.AsyncSocketClient
{
    public class DSCClientErrorEventArgs : EventArgs
    {
        public SocketException exception;

        public DSCClientErrorEventArgs(SocketException e)
        {
            this.exception = e;            
        }
    }
}

