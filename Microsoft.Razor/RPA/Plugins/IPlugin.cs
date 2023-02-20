using Microsoft.Razor.RPA.Common.Message;
using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Plugins
{
    /// <summary>
    /// Plugin Interface For Tcp Handler
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Version Of Plugin
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Name Of Plugin
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Handler Of Tcp Request
        /// </summary>
        public IHandler? Handler { get; set; }

        /// <summary>
        /// Handle Tcp Request
        /// </summary>
        /// <param name="msg">Content Message</param>
        /// <param name="clientId">TCP Client Id</param>
        /// <returns></returns>
        public void Handle(ContentMessage content, int clientId);

        /// <summary>
        /// Handle Tcp Request Async
        /// </summary>
        /// <param name="msg">Content Message</param>
        /// <param name="clientId">TCP Client Id</param>
        /// <returns></returns>
        public Task HandleAsync(ContentMessage content, int clientId);
    }
}
