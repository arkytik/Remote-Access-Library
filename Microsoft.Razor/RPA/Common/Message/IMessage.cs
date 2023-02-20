using Microsoft.Razor.RPA.Common.Entity;
using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Common.Message
{
    /// <summary>
    /// Message State
    /// </summary>
    [Flags]
    public enum MessageState
    {
        None = 0,
        Handling = 1
    }

    /// <summary>
    /// Represent Of Message Design
    /// </summary>
    public interface IMessage
    {
        public int Size { get; set; }
        public byte[] Data { get; set; }
        public MessageState State { get; set; }

        /// <summary>
        /// Function For Read Message From Client
        /// </summary>
        /// <param name="handler">TCP Handler Object</param>
        /// <param name="client">TCP Client</param>
        /// <returns>True If Read Sucssesful, otherwise False</returns>
        public ValueTask<bool> ReadAsync(IHandler handler, TCPEntity client);

        /// <summary>
        /// Function For Write Message To Client
        /// </summary>
        /// <param name="handler">TCP Handler Object</param>
        /// <param name="client">TCP Client</param>
        /// <returns>True If Write Sucssesful, otherwise False</returns>
        public ValueTask<bool> WriteAsync(IHandler handler, TCPEntity client);

        /// <summary>
        /// Reset Message To Default
        /// </summary>
        public void Reset();

        /// <summary>
        /// Function With Check Message Data
        /// </summary>
        /// <returns>True, If Message Exist And Correct, Otherwise False</returns>
        public bool IsNullOrEmpty();
    }
}
