using System.Drawing;

using Microsoft.Razor.RPA.Common.Message;
using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Plugins.Capture.Handlers
{
    /// <summary>
    /// Capture Client Handler
    /// </summary>
    public class CaptureClientHandler : IPlugin
    {
        public int Version => 1;

        public string Name => "Capture-Client";

        public IHandler? Handler { get; set; }

        /// <summary>
        /// Handle Tcp Request
        /// </summary>
        /// <param name="content">Content Message</param>
        /// <param name="clientId">Tcp Client Index</param>
        public virtual async void Handle(ContentMessage content, int clientId) => await HandleAsync(content, clientId);

        /// <summary>
        /// Handle Tcp Request Async
        /// </summary>
        /// <param name="content">Content Message</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns></returns>
        public virtual async Task HandleAsync(ContentMessage content, int clientId)
        {
            Task t = Task.Run(() =>
            {
                if (content.Type == ContentType.Image && content.From == ContentFrom.Server)
                {
                    using (Image? screen = content.GetContent<Image>())
                    {
                        UpdateScreen(screen);
                    }
                }
            });

            await t;
        }

        /// <summary>
        /// Start Video Streaming
        /// </summary>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns>True If Wrote, Otherwise False</returns>
        public virtual async ValueTask<bool> RequestStartStreaming(int clientId) => await ContentMessage.CreateAndWriteRequest(RequestType.StartVideoStream, Handler, clientId);

        /// <summary>
        /// Stop Video Streaming
        /// </summary>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns>True If Wrote, Otherwise False</returns>
        public virtual async ValueTask<bool> RequestStopStreaming(int clientId) => await ContentMessage.CreateAndWriteRequest(RequestType.StopVideoStream, Handler, clientId);

        /// <summary>
        /// Update Screen By New Image
        /// </summary>
        /// <param name="screenData">New Screen Image In Bytes</param>
        public virtual void UpdateScreen(Image? screen)
        {
            if (screen == null) return;
        }
    }
}
