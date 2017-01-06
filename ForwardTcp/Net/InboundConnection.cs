namespace ForwardTcp.Net
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class InboundConnection
    {
        private readonly int backlog;
        private readonly TcpListener tcpListener;
        private readonly byte[] buffer = new byte[10240];

        public InboundConnection(IPEndPoint endPoint, int backlog = 100)
        {
            this.backlog = backlog;
            this.tcpListener = new TcpListener(endPoint);
        }

        public event EventHandler<ConnectionEventArgs> Connected;

        public event EventHandler<ExceptionEventArgs> ConnectionError;

        public event EventHandler<MessageEventArgs> DataReceived;
        
        public event EventHandler<ExceptionEventArgs> ReceiveError;

        public void Listen(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                this.tcpListener.Start(this.backlog);

                do
                {
                    try
                    {
                        var tcpClient = this.tcpListener.AcceptTcpClient();

                        this.Connected?.Invoke(this, new ConnectionEventArgs(tcpClient.Client.RemoteEndPoint));

                        using (var connection = new SocketConnection(tcpClient))
                        {
                            connection.ReceiveError += this.ConnectionOnReceiveError;

                            int bytesReceived;
                            do
                            {
                                bytesReceived = connection.Receive(this.buffer, 0, this.buffer.Length);

                                if (bytesReceived > 0)
                                {
                                    var data = new byte[bytesReceived];
                                    Buffer.BlockCopy(this.buffer, 0, data, 0, bytesReceived);

                                    this.DataReceived?.Invoke(
                                        this,
                                        new MessageEventArgs(connection.InnerClient.Client.RemoteEndPoint, data));
                                }

                                if (cancellationToken.IsCancellationRequested)
                                    break;

                            } while (bytesReceived != 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ReceiveError?.Invoke(this, new ExceptionEventArgs(ex));
                    }

                    if (cancellationToken.IsCancellationRequested)
                        break;

                } while (true);

            }
            catch (SocketException ex)
            {
                this.ConnectionError?.Invoke(this, new ExceptionEventArgs(ex));
            }
        }

        private void ConnectionOnReceiveError(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            this.ReceiveError?.Invoke(this, exceptionEventArgs);
        }
    }
}