namespace ForwardTcp.Net
{
    using System.Net;

    public class MessageEventArgs : ConnectionEventArgs
    {
        public byte[] Data { get; set; }

        public MessageEventArgs(EndPoint remoteEndPoint, byte[] data)
            : base(remoteEndPoint)
        {
            this.Data = data;
        }
    }
}