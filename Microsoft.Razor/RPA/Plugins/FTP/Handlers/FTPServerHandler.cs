using Microsoft.Razor.RPA.Common.Handlers;
using Microsoft.Razor.RPA.Common.Message;
using System.Text;

namespace Microsoft.Razor.RPA.Plugins.FTP.Handlers
{
    /// <summary>
    /// Provides Functions For FTP Server
    /// </summary>
    public class FTPServerHandler : IPlugin
    {
        public int Version => 1;

        public string Name => "FTP-Server";

        public IHandler? Handler { get; set; }

        /// <summary>
        /// Handle FTP Request
        /// </summary>
        /// <param name="content"></param>
        /// <param name="clientId"></param>
        public virtual async void Handle(ContentMessage content, int clientId) => await HandleAsync(content, clientId);

        /// <summary>
        /// Handle FTP Requests Async
        /// </summary>
        /// <param name="content"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public virtual async Task HandleAsync(ContentMessage content, int clientId)
        {
            if (content.From == ContentFrom.Client)
            {
                switch (content.Type)
                {
                    case ContentType.File:
                        await FileExchanger.DownloadFile(Handler, content, clientId);
                        break;

                    case ContentType.Text:
                        await HandleTextAsync(content, clientId);
                        break;
                }
            }
        }

        /// <summary>
        /// Handle Texted Content
        /// </summary>
        /// <param name="content">Contented Message</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns></returns>
        protected virtual async Task HandleTextAsync(ContentMessage content, int clientId)
        {
            if (content.IsNullOrEmpty() || content.Type != ContentType.Text) return;

            string? action = string.Empty, args = string.Empty;

            FTPCommon.GetAction(content.GetContent<string>(), ref action, ref args);

            switch (action)
            {
                case FTPCommon.FTPFilesAction:
                    await GetFilesAndWriteAsync(args, clientId);
                    break;

                case FTPCommon.FTPDownloadAction:
                    await FileExchanger.UploadFile(args, Handler, clientId, ContentFrom.Server);
                    break;
            }
        }

        /// <summary>
        /// Get Files List And Send It To Client
        /// </summary>
        /// <param name="root"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        protected virtual async Task GetFilesAndWriteAsync(string? root, int clientId)
        {
            string? fltree = string.Empty;

            if (string.IsNullOrEmpty(root))
            {
                fltree = FileTree.GetDrivesAsString();
            }
            else
            {
                fltree = FileTree.GetEntitiesAsString(root);
            }

            if (!string.IsNullOrEmpty(fltree))
            {
                string? flaction = FTPCommon.PackAction(FTPCommon.FTPFilesAction, fltree);

                if (!string.IsNullOrEmpty(flaction))
                {
                    byte[] flActionBytes = Encoding.UTF8.GetBytes(flaction);

                    if (flActionBytes != null && flActionBytes.Length > 0)
                    {
                        await ContentMessage.CreateAndWriteText(flaction, Handler, clientId, ContentFrom.Server);
                    }
                }
            }
        }
    }
}
