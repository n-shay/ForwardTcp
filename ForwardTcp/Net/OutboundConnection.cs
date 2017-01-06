namespace ForwardTcp.Net
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class OutboundConnection
    { 
        private readonly IPEndPoint endPoint;
        private SocketConnection connection;
        
        public OutboundConnection(IPEndPoint endPoint)
        {
            if (endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));

            this.endPoint = endPoint;
        }

        public event EventHandler<ConnectionEventArgs> Connected;

        public event EventHandler<ExceptionEventArgs> ConnectionError;

        public event EventHandler<MessageEventArgs> DataSent;

        public void Send(byte[] data)
        {
            if (this.IsConnected() || this.Connect())
            {
                this.connection.Send(data);

                this.DataSent?.Invoke(
                    this,
                    new MessageEventArgs(this.connection.InnerClient.Client.RemoteEndPoint, data));
            }
        }

        protected bool Connect()
        {
            try
            {
                if (this.connection != null)
                {
                    this.connection.Disconnect();
                    this.connection = null;
                }

                var tcpClient = new TcpClient();
                tcpClient.Connect(this.endPoint);

                this.Connected?.Invoke(this, new ConnectionEventArgs(tcpClient.Client.RemoteEndPoint));

                this.connection = new SocketConnection(tcpClient);
                
                return true;
            }
            catch (Exception ex)
            {
                this.ConnectionError?.Invoke(this, new ExceptionEventArgs(ex));
            }

            return false;
        }

        protected bool IsConnected()
        {
            return this.connection != null && this.connection.IsConnected();
        }

        public override string ToString()
        {
            return $"{this.endPoint.Address}:{this.endPoint.Port}";
        }
    }
}