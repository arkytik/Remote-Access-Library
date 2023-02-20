using Microsoft.Razor.RPA.Common.List;
using Microsoft.Razor.RPA.Common.Message;

namespace Microsoft.Razor.RPA.Common.Handlers
{
    /// <summary>
    /// Handler Interface With Read-Write Override Functions
    /// </summary>
    public interface IHandler : IDisposable
    {
        /// <summary>
        /// Run Status
        /// </summary>
        public bool IsRun { get; set; }

        /// <summary>
        /// Evented Clients List
        /// </summary>
        public ClientsList Clients { get; set; }

        /// <summary>
        /// On Receive Handler
        /// </summary>
        public Action<ContentMessage, int>? OnReceiveHandler { get; set; }

        /// <summary>
        /// On New Data Received Invoker
        /// </summary>
        /// <param name="msg">Message Received</param>
        /// <param name="clientId">Client Sender Id</param>
        public void OnReceive(ContentMessage msg, int clientId);

        /// <summary>
        /// Begin Tcp Request Handles
        /// </summary>
        public void HandleClients();

        /// <summary>
        /// Functions With Received Handles Function
        /// </summary>
        /// <returns></returns>
        public Task HandleClientsAsync();
    }
}
