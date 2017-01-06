namespace ForwardTcp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using ForwardTcp.Net;

    public class Program
    {
        public static void Main(string[] args)
        {
            var utility = new UtilityArguments(args);

            if (utility.InboundPort == null || 
                utility.OutboundIpAddress1 == null || 
                utility.Contains("help") || 
                utility.Contains("h") || 
                utility.Contains("?"))
            {
                Console.WriteLine("Forwards an inbound TCP connection to one or more outbound TCP connection.");
                Console.WriteLine();
                Console.WriteLine("USAGE:");
                Console.WriteLine(@"    splittcp -in 80 -out1 ""192.168.1.1""");
                Console.WriteLine(@"                    [-out2 ""192.168.1.2"" |");
                Console.WriteLine(@"                     -out3 ""192.168.1.3"" |");
                Console.WriteLine(@"                     -out4 ""192.168.1.4"" |");
                Console.WriteLine(@"                     -out5 ""192.168.1.5""]");
                Console.WriteLine();
                Console.WriteLine("REQUIRED:");
                Console.WriteLine("    -in [tcp port]         Inbound TCP port to listen to");
                Console.WriteLine("    -out1 [ip address]     IP address (or host name) of 1st outbound TCP connection");

                Console.WriteLine();
                Console.WriteLine("OPTIONAL:");
                Console.WriteLine("    -out2 [ip address]     IP address (or host name) of 2nd outbound TCP connection");
                Console.WriteLine("    -out3 [ip address]     IP address (or host name) of 3rd outbound TCP connection");
                Console.WriteLine("    -out4 [ip address]     IP address (or host name) of 4th outbound TCP connection");
                Console.WriteLine("    -out5 [ip address]     IP address (or host name) of 5th outbound TCP connection");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");

                Console.ReadKey();
                return;
            }

            // validate port
            if (utility.InboundPort == 0)
            {
                Console.WriteLine($"Port is not a valid (0 < port <= {ushort.MaxValue})");
                return;
            }

            // validate ip address
            IPAddress out1, out2, out3, out4, out5;
            if (!TryParseIpAddress(utility.OutboundIpAddress1, out out1)
                || !TryParseIpAddress(utility.OutboundIpAddress2, out out2)
                || !TryParseIpAddress(utility.OutboundIpAddress3, out out3)
                || !TryParseIpAddress(utility.OutboundIpAddress4, out out4)
                || !TryParseIpAddress(utility.OutboundIpAddress5, out out5))
                return;

            // set up connections
            var inbound = new InboundConnection(new IPEndPoint(IPAddress.Any, utility.InboundPort.Value));
            var outbounds = new List<OutboundConnection>();
            AddOutboundConnection(new IPEndPoint(out1, utility.InboundPort.Value), outbounds);
            if (out2 != null) AddOutboundConnection(new IPEndPoint(out2, utility.InboundPort.Value), outbounds);
            if (out3 != null) AddOutboundConnection(new IPEndPoint(out3, utility.InboundPort.Value), outbounds);
            if (out4 != null) AddOutboundConnection(new IPEndPoint(out4, utility.InboundPort.Value), outbounds);
            if (out5 != null) AddOutboundConnection(new IPEndPoint(out5, utility.InboundPort.Value), outbounds);

            inbound.Connected += InboundOnConnected;
            inbound.ConnectionError += InboundOnConnectionError;
            inbound.DataReceived += (o, e) =>
                {
                    Console.WriteLine($"[{DateTime.Now:s}] Received {e.Data.Length} bytes (Source={((IPEndPoint)e.RemoteEndPoint).Address}:{((IPEndPoint)e.RemoteEndPoint).Port}).");

                    ForwardToOutbounds(e.Data, outbounds); 
                };

            var cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine($"[{DateTime.Now:s}] Starting...");

            var task = Task.Run(() => inbound.Listen(cancellationTokenSource.Token));

            // Listen for a key response.
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);

                try
                {
                    //switch (keyInfo.Key)
                    //{
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:s}] Error executing {keyInfo.Key}. Exception: {ex}");
                }
            } while (keyInfo.Key != ConsoleKey.Q);

            if (!task.IsCompleted)
            {
                Console.WriteLine($"[{DateTime.Now:s}] Stopping...");
                cancellationTokenSource.Cancel();

                task.Wait(TimeSpan.FromSeconds(8));
            }
        }

        private static void InboundOnConnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now:s}] Connected to inbound ({((IPEndPoint)e.RemoteEndPoint).Address}:{((IPEndPoint)e.RemoteEndPoint).Port})");
        }

        private static void AddOutboundConnection(IPEndPoint endPoint, ICollection<OutboundConnection> outbounds)
        {
            var outbound = new OutboundConnection(endPoint);
            outbound.DataSent += OutboundOnDataSent;
            outbound.Connected += OutboundOnConnected;
            outbound.ConnectionError += OutboundOnConnectionError;
            outbounds.Add(outbound);
        }

        private static void OutboundOnConnected(object sender, ConnectionEventArgs args)
        {
            Console.WriteLine($"[{DateTime.Now:s}] Connected (Dest={sender})");
        }

        private static void OutboundOnDataSent(object sender, MessageEventArgs args)
        {
            Console.WriteLine($"[{DateTime.Now:s}] Sent {args.Data.Length} bytes (Dest={sender})");
        }

        private static void InboundOnConnectionError(object sender, ExceptionEventArgs args)
        {
            Console.WriteLine($"[{DateTime.Now:s}] Error connecting to inbound: {args.InnerException.Message}");
        }

        private static void OutboundOnConnectionError(object sender, ExceptionEventArgs args)
        {
            Console.WriteLine($"[{DateTime.Now:s}] Error connecting to {sender}: {args.InnerException.Message}");
        }

        private static void ForwardToOutbounds(byte[] data, List<OutboundConnection> outbounds)
        {
            outbounds.ForEach(
                c =>
                    {
                        c.Send(data);
                    });
        }

        private static bool TryParseIpAddress(string ip, out IPAddress ipAddress)
        {
            if (string.IsNullOrEmpty(ip))
            {
                ipAddress = null;
                return true;
            }

            IPAddress ipAddress2 = null;
            try
            {
                ipAddress2 = Dns.GetHostAddresses(ip).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
            }

            if (ipAddress2 == null)
            {
                Console.WriteLine($"IP address '{ip}' cannot be resolved.");
                ipAddress = null;
                return false;
            }

            ipAddress = ipAddress2;
            return true;
        }
    }
}
