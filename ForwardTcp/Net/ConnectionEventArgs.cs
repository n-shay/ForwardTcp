namespace ForwardTcp.Net
{
    using System;
    using System.Net;

    public class ConnectionEventArgs : EventArgs
    {
        public EndPoint RemoteEndPoint { get; }

        public ConnectionEventArgs(EndPoint remoteEndPoint)
        {
            this.RemoteEndPoint = remoteEndPoint;
        }
    }
}