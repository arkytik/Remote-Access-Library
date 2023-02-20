using System.Net;
using System.Net.Sockets;

using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Client
{
    /// <summary>
    /// Provides Tcp Base Client Functions
    /// </summary>
    public class BaseClient : IClient
    {
        public IPAddress IP { get; private set; }
        public int Port { get; private set; }

        public IHandler Handler { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">Server IP</param>
        /// <param name="port">Server Port</param>
        /// <param name="handler">Tcp Handler</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public BaseClient(IPAddress ip, int port, IHandler handler) 
        {
            if (ip == null) throw new ArgumentNullException(nameof(ip));

            if (port <= 0 || port >= 65536) throw new ArgumentOutOfRangeException(nameof(port));

            if (handler == null) throw new ArgumentNullException(nameof(handler));

            IP = ip;
            Port = port;

            Handler = handler;

            if (!Handler.IsRun)
            {
                Handler.IsRun = true;

                Handler.HandleClients();
            }
        }

        /// <summary>
        /// Constructor With Domain In Str
        /// </summary>
        /// <param name="domain">Domain In Str</param>
        /// <param name="handler">Tcp Handler</param>
        public BaseClient(string domain, IHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));

            string[] parts = domain.Split(':');

            if (parts == null || parts.Length < 2) throw new AbandonedMutexException(nameof(parts));

            string ipAsStr = string.Join(":", parts.Take(parts.Length - 1));
            string portAsStr = parts.Last().Trim();

            IP = IPAddress.Parse(ipAsStr);
            Port = int.Parse(portAsStr);

            Handler = handler;

            if (!Handler.IsRun)
            {
                Handler.IsRun = true;

                Handler.HandleClients();
            }
        }

        /// <summary>
        /// Start Client Work
        /// </summary>
        public virtual async void Start() => await StartAsync();

        /// <summary>
        /// Start Client Work Async
        /// </summary>
        /// <returns>True If Connected To Server, Otherwise False</returns>
        public virtual async ValueTask<bool> StartAsync()
        {
            try
            {
                TcpClient client = new();
                await client.ConnectAsync(IP, Port);

                Handler.Clients.Add(new(client));

                return client.Connected;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Free Memory
        /// </summary>
        public void Dispose() => Handler?.Dispose();
    }
}
