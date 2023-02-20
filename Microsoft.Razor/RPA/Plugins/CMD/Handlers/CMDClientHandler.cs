using Microsoft.Razor.RPA.Common.Handlers;
using Microsoft.Razor.RPA.Common.Message;

namespace Microsoft.Razor.RPA.Plugins.CMD.Handlers
{
    /// <summary>
    /// Provides Functions For Handling Server CMD Request
    /// </summary>
    public class CMDClientHandler : IPlugin
    {
        public int Version => 1;

        public string Name => "CMD-Client";

        public IHandler? Handler { get; set; }

        /// <summary>
        /// The Action For Executing On Server Bat Code Notify
        /// </summary>
        public Action? OnExecuted { get; set; }

        /// <summary>
        /// Handle CMD Request From Server
        /// </summary>
        /// <param name="content"></param>
        /// <param name="clientId"></param>
        public virtual async void Handle(ContentMessage content, int clientId) => await HandleAsync(content, clientId);

        /// <summary>
        /// Handle CMD Request From Server Async
        /// </summary>
        /// <param name="content"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public virtual async Task HandleAsync(ContentMessage content, int clientId)
        {
            Task t = Task.Run(() =>
            {
                if (content.From == ContentFrom.Server && content.Type == ContentType.Request)
                {
                    RequestType? request = content.GetContent<RequestType>();

                    if (request != null)
                    {
                        switch (request)
                        {
                            case RequestType.ExecutedBatCode:
                                OnExecuted?.Invoke();
                                break;
                        }
                    }
                }
            });

            await t;
        }

        /// <summary>
        /// Execute CMD Code
        /// </summary>
        /// <param name="code">Bat Code</param>
        /// <returns>True If Executed, Otherwise False</returns>
        public virtual async ValueTask<bool> ExecuteCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code)) return false;

                return await WinCommander.ExecuteAsync(code);
            }
            catch
            {
                return false;
            }
        }
    }
}
