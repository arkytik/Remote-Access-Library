using Microsoft.Razor.RPA.Common.Handlers;
using Microsoft.Razor.RPA.Common.Message;
using System.Text;

namespace Microsoft.Razor.RPA.Plugins.FTP.Handlers
{
    /// <summary>
    /// FTP Client Addon
    /// </summary>
    public class FTPClientHandler : IPlugin
    {
        public int Version => 1;

        public string Name => "FTP-Client";

        public IHandler? Handler { get; set; }

        /// <summary>
        /// Handle FTP Request From Server
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="clientId">TCP Client Index</param>
        public async void Handle(ContentMessage content, int clientId) => await HandleAsync(content, clientId);

        /// <summary>
        /// Handle FTP Request From Server Async
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="clientId">TCP Client Index</param>
        /// <returns></returns>
        public async Task HandleAsync(ContentMessage content, int clientId)
        {
            if (content.From == ContentFrom.Server)
            {
                switch (content.Type)
                {
                    case ContentType.File:
                        await FileExchanger.DownloadFile(Handler, content, clientId);
                        break;

                    case ContentType.Text:
                        HandleText(content, clientId);
                        break;
                }
            }
        }

        /// <summary>
        /// Request Get Files
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetFiles(int clientId, string? root = null)
        {
            try
            {
                if (string.IsNullOrEmpty(root)) return false;

                string? flaction = FTPCommon.PackAction(FTPCommon.FTPFilesAction, root);

                if (!string.IsNullOrEmpty(flaction))
                {
                    return await ContentMessage.CreateAndWriteText(flaction, Handler, clientId);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Begin Download File From Server
        /// </summary>
        /// <param name="filepath">Path To File On Server</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns></returns>
        public async Task DownloadFile(string filepath, int clientId)
        {
            if (string.IsNullOrEmpty(filepath) || (Handler == null || Handler.Clients[clientId] == null)) return;

            string? flaction = FTPCommon.PackAction(FTPCommon.FTPDownloadAction, filepath);

            if (!string.IsNullOrEmpty(flaction))
            {
                byte[] flActionBytes = Encoding.UTF8.GetBytes(flaction);

                if (flActionBytes != null && flActionBytes.Length > 0)
                {
                    await ContentMessage.CreateAndWriteText(flaction, Handler, clientId, ContentFrom.Client);
                }
            }
        }

        /// <summary>
        /// Handle Texted Content
        /// </summary>
        /// <param name="content">Contented Message</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns></returns>
        protected virtual void HandleText(ContentMessage content, int clientId)
        {
            if (content.IsNullOrEmpty() || content.Type != ContentType.Text) return;

            string? action = string.Empty, args = string.Empty;

            FTPCommon.GetAction(content.GetContent<string>(), ref action, ref args);

            switch (action)
            {
                case FTPCommon.FTPFilesAction:
                    UpdateFileList(args);
                    break;
            }
        }

        /// <summary>
        /// Update File List With New Tree
        /// </summary>
        /// <param name="fltree"></param>
        public virtual void UpdateFileList(string? fltree) { }
    }
}
