using Microsoft.Razor.RPA.Plugins;
using Microsoft.Razor.RPA.Common.List;
using Microsoft.Razor.RPA.Common.Message;

namespace Microsoft.Razor.RPA.Common.Handlers
{
    /// <summary>
    /// Simple BaseHandler For TCP Server/Client
    /// </summary>
    public class BaseHandler : IHandler
    {
        public int Delay { get; set; }

        public bool IsRun { get; set; }

        public ClientsList Clients { get; set; }

        public PluginLoader Plugins { get; protected set; }

        public Action<ContentMessage, int>? OnReceiveHandler { get; set; }

        /// <summary>
        /// On Receive Data Callback
        /// </summary>
        /// <param name="msg">IMessage Represent</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void OnReceive(ContentMessage msg, int clientId)
        {
            if (msg == null || clientId == -1) return;

            OnReceiveHandler?.Invoke(msg, clientId);
        }

        /// <summary>
        /// Constructor Of Base Handler
        /// </summary>
        public BaseHandler(int delay = 10)
        {
            IsRun = false;
            Delay = delay;

            Clients = new();
            Plugins = new(this);
        }

        /// <summary>
        /// Begin Tcp Request Handles
        /// </summary>
        public virtual async void HandleClients() => await HandleClientsAsync();

        /// <summary>
        /// Function Where Tcp Clients Begin Handles Async
        /// </summary>
        /// <returns></returns>
        public virtual async Task HandleClientsAsync()
        {
            if (IsRun && Plugins != null) Plugins.Load();

            while (IsRun)
            {
                if (Clients.Count > 0)
                {
                    Parallel.ForEach(Clients, async client =>
                    {
                        if (client.Stream != null && client.Stream.DataAvailable)
                        {
                            bool isRead = await client.MainContent.ReadAsync(this, client);

                            if (isRead) OnReceive(client.MainContent, Clients.LastIndexOf(client));
                        }
                        else if (client != null && client.Stream == null)
                        {
                            client.Dispose();

                            Clients.Remove(client);
                        }

                    });
                }

                await Task.Delay(Delay);
            }
        }

        /// <summary>
        /// Dispose Clients And Free Memory
        /// </summary>
        public void Dispose()
        {
            Clients?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
