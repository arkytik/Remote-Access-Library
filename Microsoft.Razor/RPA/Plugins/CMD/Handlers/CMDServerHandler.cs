using Microsoft.Razor.RPA.Common.Handlers;
using Microsoft.Razor.RPA.Common.Message;

namespace Microsoft.Razor.RPA.Plugins.CMD.Handlers
{
    /// <summary>
    /// Provides Functions For Cmd Server Plugin
    /// </summary>
    public class CMDServerHandler : IPlugin
    {
        public int Version => 1;

        public string Name => "CMD-Server";

        public IHandler? Handler { get; set; }

        /// <summary>
        /// Handle Cmd Request
        /// </summary>
        /// <param name="content"></param>
        /// <param name="clientId"></param>
        public virtual async void Handle(ContentMessage content, int clientId) => await HandleAsync(content, clientId);

        /// <summary>
        /// Handle Cmd Request Async
        /// </summary>
        /// <param name="content"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public virtual async Task HandleAsync(ContentMessage content, int clientId)
        {
            if (content.From == ContentFrom.Client && content.Type == ContentType.BatCode)
            {
                string? code = content.GetContent<string>();

                if (!string.IsNullOrEmpty(code))
                {
                    bool isExecuted = await WinCommander.ExecuteAsync(code);

                    if (isExecuted) await ContentMessage.CreateAndWriteRequest(RequestType.ExecutedBatCode, Handler, clientId);
                }
            }
        }
    }
}
