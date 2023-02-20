using Microsoft.Razor.RPA.Common.Entity;
using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Common.Message
{
    /// <summary>
    /// Simple Framed Message - Represent Of IMessage Design
    /// </summary>
    public class FrameMessage : IMessage
    {
        public const int MaxSize = 512 * 1024;

        public int Size { get; set; }
        public byte[] Data { get; set; }
        public MessageState State { get; set; } = MessageState.None;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data For Message</param>
        public FrameMessage(byte[]? data = null)
        {
            if (data == null) data = new byte[0];

            Data = data;
            Size = Data.Length;
        }

        /// <summary>
        /// Read Our Message Async
        /// </summary>
        /// <param name="handler">Ref To TCP Handler</param>
        /// <param name="client">TCP Entity</param>
        /// <returns>True if read was sucsessful, otherwise False</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async ValueTask<bool> ReadAsync(IHandler handler, TCPEntity client)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!handler.Clients.Contains(client)) return false;

            if (State != MessageState.None) return false;

            Size = await ReadInt(client);

            if (Size <= 0 || Size > MaxSize) return false;

            byte[]? data = await client.ReadAsync(Size);

            if (data != null)
            {
                Data = data;

                bool result = Data.Length == Size;

                if (result) State = MessageState.Handling;

                return result;
            }
            else return false;
        }

        /// <summary>
        /// Send Message To Client
        /// </summary>
        /// <param name="handler">TCP Handler Ref</param>
        /// <param name="client">TCP Entity</param>
        /// <returns>True if write was sucsessful, otherwise False</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async ValueTask<bool> WriteAsync(IHandler handler, TCPEntity client)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!handler.Clients.Contains(client)) return false;

            if (State != MessageState.None) return false;

            if (!IsNullOrEmpty())
            {
                byte[] buffer = new byte[Size + 4];

                byte[] sizeInBytes = BitConverter.GetBytes(Size);

                buffer[0] = sizeInBytes[0];
                buffer[1] = sizeInBytes[1];
                buffer[2] = sizeInBytes[2];
                buffer[3] = sizeInBytes[3];

                Data.CopyTo(buffer, 4);

                return await client.WriteAsync(buffer);
            }
            else return false;
        }

        /// <summary>
        /// Read Int Helper Function
        /// </summary>
        /// <param name="client">TCP Entity</param>
        /// <returns>-1 If Error With Network Reading, otherwise Message Size</returns>
        protected virtual async Task<int> ReadInt(TCPEntity client)
        {
            int size = -1;

            if (!client.Connected) return size;

            byte[]? sizeInBytes = await client.ReadAsync(4);

            if (sizeInBytes != null && sizeInBytes.Length == 4) size = BitConverter.ToInt32(sizeInBytes);

            return size;
        }

        /// <summary>
        /// Reset Message To Default Values
        /// </summary>
        public virtual void Reset()
        {
            Data = new byte[0];
            Size = Data.Length;

            State = MessageState.None;
        }

        /// <summary>
        /// Checking Message Data
        /// </summary>
        /// <returns>True, If Message Exist And Correct, Otherwise False</returns>
        public virtual bool IsNullOrEmpty()
        {
            return (this == null || Data == null) || (Size < 0 || Data.Length != Size);
        }
    }
}
