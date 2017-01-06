namespace ForwardTcp.Net
{
    using System;

    public class ExceptionEventArgs : EventArgs
    {
        public Exception InnerException { get; }

        public ExceptionEventArgs(Exception innerException)
        {
            this.InnerException = innerException;
        }
    }
}