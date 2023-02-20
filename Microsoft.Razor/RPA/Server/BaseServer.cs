using System.Net;
using System.Net.Sockets;

using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Server
{
    /// <summary>
    /// Custom Server With Accept, Handle Clients Functions
    /// </summary>
    public class BaseServer : IServer
    {
        public bool IsRun { get; private set; }

        public IHandler Handler { get; private set; }
        public TcpListener Listener { get; private set; }

        /// <summary>
        /// Constructor Of Base Server
        /// </summary>
        /// <param name="handler">Handler Class For Tcp Clients</param>
        /// <param name="ip">IP Of Server Bind</param>
        /// <param name="port">Port Of Server Bind</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public BaseServer(IHandler handler, IPAddress ip, int port)
        {
            if (ip == IPAddress.None) throw new ArgumentException(nameof(ip));

            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (port <= 0 || port > 65536) throw new ArgumentOutOfRangeException(nameof(port));

            IsRun = true;

            Handler = handler;
            Listener = new(ip, port);

            if (!Handler.IsRun)
            {
                Handler.IsRun = true;

                Handler.HandleClients();
            }
        }

        /// <summary>
        /// Start Server
        /// </summary>
        public virtual async void Start() => await StartAsync();

        /// <summary>
        /// Start Server Async
        /// </summary>
        public virtual async Task StartAsync()
        {
            Listener.Start();

            await AcceptClientsAsync();
        }

        /// <summary>
        /// Start Accept Tcp Clients
        /// </summary>
        /// <returns></returns>
        public virtual async Task AcceptClientsAsync()
        {
            while (IsRun)
            {
                TcpClient client = await Listener.AcceptTcpClientAsync();

                Handler.Clients.Add(new(client));
            }
        }

        /// <summary>
        /// Stop And Delete Server From Memory
        /// </summary>
        public void Dispose()
        {
            IsRun = false;
            Handler?.Dispose();

            Listener?.Stop();

            GC.SuppressFinalize(this);
        }
    }
}
