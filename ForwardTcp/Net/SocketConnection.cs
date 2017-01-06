namespace ForwardTcp.Net
{
    using System;
    using System.Net.Sockets;

    public class SocketConnection : IDisposable
    {
        public SocketConnection(TcpClient innerClient)
        {
            this.InnerClient = innerClient;
        }

        ~SocketConnection()
        {
            this.Dispose(false);
        }

        public TcpClient InnerClient { get; private set; }

        public event EventHandler<ExceptionEventArgs> DisconnectionError;

        public event EventHandler<ExceptionEventArgs> SendError;

        public event EventHandler<ExceptionEventArgs> ReceiveError;

        public bool IsConnected()
        {
            return this.InnerClient != null &&
                   !((this.InnerClient.Client.Poll(1000, SelectMode.SelectRead) && (this.InnerClient.Client.Available == 0)) ||
                     !this.InnerClient.Client.Connected);
        }

        public virtual void Disconnect()
        {
            try
            {
                if (this.IsConnected())
                {
                    this.InnerClient.Close();
                }
            }
            catch (Exception ex)
            {
                this.DisconnectionError?.Invoke(this, new ExceptionEventArgs(ex));
            }
            finally
            {
                this.InnerClient = null;
            }
        }

        public virtual void Send(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            try
            {
                if (this.IsConnected())
                {
                    var stream = this.InnerClient.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                this.SendError?.Invoke(this, new ExceptionEventArgs(ex));
            }
        }

        public virtual int Receive(byte[] buffer, int offset, int count)
        {
            try
            {
                if (this.IsConnected())
                {
                    var stream = this.InnerClient.GetStream();

                    return stream.Read(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                this.ReceiveError?.Invoke(this, new ExceptionEventArgs(ex));
            }

            return 0;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).

                this.InnerClient?.Close();
                this.InnerClient = null;
            }

            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }
    }
}
