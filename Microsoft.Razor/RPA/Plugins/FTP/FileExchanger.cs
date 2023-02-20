using Microsoft.Razor.RPA.Common.Message;
using Microsoft.Razor.RPA.Common.Handlers;
using System.Text;

namespace Microsoft.Razor.RPA.Plugins.FTP
{
    /// <summary>
    /// Provides Functions To Get, Create, Upload, Download Files
    /// </summary>
    public class FileExchanger
    {
        /// <summary>
        /// Max File Chunk Size In Bytes
        /// </summary>
        public const int MaxChunkSize = 32768;

        /// <summary>
        /// Try Get File Byte Data
        /// </summary>
        /// <param name="path">Folder Path</param>
        /// <returns>Bytes Of File Or Null</returns>
        public static async ValueTask<byte[]?> GetFileAsync(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) return await File.ReadAllBytesAsync(path);

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create Of Append File
        /// </summary>
        /// <param name="path">File Path</param>
        /// <param name="data">File Byte Array</param>
        /// <returns>False If Error, Otherwise True</returns>
        public static async ValueTask<bool> CreateOrAppendFile(string path, byte[] data)
        {
            if (string.IsNullOrEmpty(path)) return false;

            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Append))
                {
                    await fs.WriteAsync(data);
                }
            }
            else
            {
                string[] paths = path.Split('\\');

                if (paths != null && paths.Length > 0)
                {
                    string folderPath = path.Replace(paths.Last(), "");

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    using (FileStream fs = File.Create(path))
                    {
                        await fs.WriteAsync(data);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Upload File To Network
        /// </summary>
        /// <param name="filepath">Filepath</param>
        /// <param name="handler">Tcp Handler</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <param name="from">From Upload</param>
        /// <returns>True If Uploaded, Otherwise False</returns>
        public static async ValueTask<bool> UploadFile(string? filepath, IHandler? handler, int clientId, ContentFrom from = ContentFrom.Client)
        {
            try
            {
                if (string.IsNullOrEmpty(filepath) || handler == null || handler.Clients[clientId] == null) return false;

                var client = handler.Clients[clientId];

                var fileInfo = new FileInfo(filepath);

                if (fileInfo.Exists && client != null)
                {
                    byte[] filename = Encoding.UTF8.GetBytes(fileInfo.Name);

                    byte[] fileheader = new byte[filename.Length + 1];
                    fileheader[0] = (byte)filename.Length;

                    filename.CopyTo(fileheader, 1);

                    ContentMessage fileMessage = new(ContentType.File, from);

                    if (fileInfo.Length > MaxChunkSize)
                    {
                        using (FileStream fs = File.OpenRead(filepath))
                        {
                            while (fs.Position < fileInfo.Length)
                            {
                                int chunkSize = MaxChunkSize;

                                if ((fs.Position + chunkSize) > fileInfo.Length) chunkSize = (int)(fileInfo.Length - fs.Position);

                                fileMessage.Data = new byte[chunkSize + fileheader.Length];
                                fileheader.CopyTo(fileMessage.Data, 0);

                                await fs.ReadAsync(fileMessage.Data, fileheader.Length, chunkSize);

                                await fileMessage.WriteAsync(handler, client);

                                await Task.Delay(1);  // For CPU Optimizing
                            }
                        }
                    }
                    else
                    {
                        byte[]? data = await GetFileAsync(filepath);

                        if (data != null && data.Length > 0)
                        {
                            fileMessage.Data = new byte[data.Length + fileheader.Length];

                            fileheader.CopyTo(fileMessage.Data, 0);
                            data.CopyTo(fileMessage.Data, fileheader.Length);

                            await fileMessage.WriteAsync(handler, client);
                        }
                    }

                    GC.SuppressFinalize(fileMessage);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Download File From Network
        /// </summary>
        /// <param name="handler">Tcp Handler</param>
        /// <param name="content">Content Message</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns>True If Downloaded, Otherwise False</returns>
        public static async ValueTask<bool> DownloadFile(IHandler? handler, ContentMessage content, int clientId)
        {
            try
            {
                if (handler == null || content == null || handler.Clients[clientId] == null) return false;

                if (content.Type == ContentType.File)
                {
                    byte[]? filedata = content.GetContent<byte[]>();

                    if (filedata != null)
                    {
                        int strlen = filedata[0];

                        filedata = filedata.Skip(strlen + 1).ToArray();

                        byte[] strdata = filedata.Skip(1).Take(strlen).ToArray();

                        string filepath = Encoding.UTF8.GetString(strdata).Trim();

                        filepath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{filepath}";

                        return await CreateOrAppendFile(filepath, filedata);
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
