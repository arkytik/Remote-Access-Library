using System.Text;
using System.Drawing;

using Microsoft.Razor.RPA.Common.Entity;
using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Common.Message
{
    /// <summary>
    /// The Content Type
    /// </summary>
    [Flags]
    public enum ContentType
    {
        None = 0,
        File = 1,
        Text = 2,
        Json = 3,
        Image = 4,
        BatCode = 5,
        Request = 6,
    }

    /// <summary>
    /// The Content From
    /// </summary>
    [Flags]
    public enum ContentFrom
    {
        None = 0,
        Client = 1,
        Server = 2
    }

    /// <summary>
    /// Type Of Request Typed Content
    /// </summary>
    public enum RequestType
    {
        None = 0,
        StartVideoStream = 1,  //  Request Start Video Streaming
        StopVideoStream = 2,   //  Request Stop Video Streaming
        ExecutedBatCode = 3,   //  Bat Code Was Executed
    }

    /// <summary>
    /// Content Message Represent Based On FrameMessage
    /// </summary>
    public class ContentMessage : FrameMessage
    {
        public ContentType Type { get; private set; }
        public ContentFrom From { get; private set; }

        /// <summary>
        /// Base Constructor
        /// </summary>
        /// <param name="type">Type Of Content</param>
        /// <param name="from">Location Of Content</param>
        /// <param name="data">Content Data</param>
        public ContentMessage(ContentType type = ContentType.None, ContentFrom from = ContentFrom.None, byte[]? data = null) : base(data)
        {
            Type = type;
            From = from;
        }

        /// <summary>
        /// Return Converted Content To Type
        /// </summary>
        /// <typeparam name="T">Type Of Content</typeparam>
        /// <returns>Converted Content To T Or Null If Error</returns>
        public virtual T? GetContent<T>()
        {
            dynamic? result = null;

            switch (Type)
            {
                case ContentType.File:
                    if (typeof(T) == typeof(byte[])) result = Data;
                    break;

                case ContentType.Text:
                    if (typeof(T) == typeof(string)) result = GetContentAsText();
                    break;

                case ContentType.Json:
                    if (typeof(T) == typeof(string)) result = GetContentAsJson();
                    break;

                case ContentType.Image:
                    if (typeof(T) == typeof(Image)) result = GetContentAsImage();
                    break;

                case ContentType.Request:
                    if (typeof(T) == typeof(RequestType)) result = GetContentAsRequest();
                    break;

                case ContentType.BatCode:
                    if (typeof(T) == typeof(string)) result = GetContentAsBatCode();
                    break;
            }

            if (result != null) return (T)result;

            return default(T);
        }

        /// <summary>
        /// Convert Content To Text
        /// </summary>
        /// <returns>Content Data As String Or Null If Error</returns>
        protected virtual string? GetContentAsText()
        {
            try
            {
                if (IsNullOrEmpty()) return null;

                return Encoding.UTF8.GetString(Data);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert Content To Json
        /// </summary>
        /// <returns>Content Data As Json Or Null If Error</returns>
        protected virtual string? GetContentAsJson()
        {
            return GetContentAsText();
        }

        /// <summary>
        /// Convert Content To Image
        /// </summary>
        /// <returns>Content Data As Image Or Null If Error</returns>
        protected virtual Image? GetContentAsImage()
        {
            try
            {
                if (IsNullOrEmpty()) return null;

                using (MemoryStream stream = new(Data))
                {
                    return Image.FromStream(stream);
                }
            } 
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert Content To Request Type
        /// </summary>
        /// <returns>Content As Request Type Or None If Error</returns>
        protected virtual RequestType GetContentAsRequest()
        {
            try
            {
                if (IsNullOrEmpty()) return RequestType.None;

                return (RequestType)BitConverter.ToInt32(Data, 0);
            }
            catch
            {
                return RequestType.None;
            }
        }

        /// <summary>
        /// Convert Content To BatCode
        /// </summary>
        /// <returns>Content Data As BatCode Or Null If Error</returns>
        protected virtual string? GetContentAsBatCode()
        {
            return GetContentAsText();
        }

        /// <summary>
        /// Read Content Message Async
        /// </summary>
        /// <param name="handler">TCP Handler</param>
        /// <param name="client">TCP Entity</param>
        /// <returns>True If Read Sucsessful, Otherwise False</returns>
        public override async ValueTask<bool> ReadAsync(IHandler handler, TCPEntity client)
        {
            bool result = await base.ReadAsync(handler, client);

            if (result && Data.Length >= 2)
            {
                From = (ContentFrom)Data[0];
                Type = (ContentType)Data[1];

                if (Data.Length >= 2)
                {
                    Data = Data.Skip(2).ToArray();

                    Size = Data.Length;
                }
            }
            else result = false;

            return result;
        }

        /// <summary>
        /// Write Data From Message To Network
        /// </summary>
        /// <param name="handler">TCP Handler</param>
        /// <param name="client">TCP Entity</param>
        /// <returns>True If Write Sucsessful, Otherwise False</returns>
        public override async ValueTask<bool> WriteAsync(IHandler handler, TCPEntity client)
        {
            if (IsNullOrEmpty()) return false;

            byte[] data = new byte[Data.Length + 2];
            data[0] = (byte)From;
            data[1] = (byte)Type;

            Data.CopyTo(data, 2);

            Data = data;
            Size = Data.Length;

            return await base.WriteAsync(handler, client);
        }

        /// <summary>
        /// Reset Content Message To Default
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            Type = ContentType.None;
            From = ContentFrom.None;
        }

        /// <summary>
        /// Create Content Message With Text And Write
        /// </summary>
        /// <param name="handler">Tcp Handler</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns></returns>
        public static async ValueTask<bool> CreateAndWriteText(string text, IHandler? handler, int clientId, ContentFrom from = ContentFrom.Client)
        {
            if (handler == null || handler.Clients == null) return false;

            TCPEntity? client = handler.Clients[clientId];

            if (client != null && !string.IsNullOrEmpty(text))
            {
                ContentMessage msg = new(ContentType.Text, from, Encoding.UTF8.GetBytes(text));

                return await msg.WriteAsync(handler, client);
            }
            else return false;
        }

        /// <summary>
        /// Create Content Message With Request And Write
        /// </summary>
        /// <param name="handler">Tcp Handler</param>
        /// <param name="clientId">Tcp Client Index</param>
        /// <returns></returns>
        public static async ValueTask<bool> CreateAndWriteRequest(RequestType request, IHandler? handler, int clientId, ContentFrom from = ContentFrom.Client)
        {
            if (handler == null || handler.Clients == null) return false;

            TCPEntity? client = handler.Clients[clientId];

            if (client != null)
            {
                ContentMessage msg = new(ContentType.Request, from, BitConverter.GetBytes((int)request));

                return await msg.WriteAsync(handler, client);
            }
            else return false;
        }
    }
}
