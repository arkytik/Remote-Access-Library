using System.Net.Sockets;

using Microsoft.Razor.RPA.Common.Message;

namespace Microsoft.Razor.RPA.Common.Entity
{
    /// <summary>
    /// State Of Entity
    /// </summary>
    [Flags]
    public enum EntityState
    {
        None = 0,
        ReadMode = 1,
        WriteMode = 2,
    }

    /// <summary>
    /// TCP Client Override With State
    /// </summary>
    public class TCPEntity : IDisposable
    {
        /// <summary>
        /// Tcp Client As Entity
        /// </summary>
        public TcpClient Entity { get; set; }

        /// <summary>
        /// Entity Mode/Status
        /// </summary>
        public EntityState Status { get; set; }

        /// <summary>
        /// Main Message Content
        /// </summary>
        public ContentMessage MainContent { get; protected set; }

        /// <summary>
        /// Return True If Connected, Otherwise False
        /// </summary>
        public bool Connected { get => Entity != null && Entity.Connected; }

        /// <summary>
        /// Return NetworkStream If Connected, Otherwise Null
        /// </summary>
        public NetworkStream? Stream { get => Connected ? Entity.GetStream() : null; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entity">TCP Client</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TCPEntity(TcpClient entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Entity = entity;
            MainContent = new();

            Status = EntityState.None;
        }

        /// <summary>
        /// Read Bytes From Buffer And Change State
        /// </summary>
        /// <param name="count">Lenght Of Buffer</param>
        /// <returns>Buffer Or Null</returns>
        public async ValueTask<byte[]?> ReadAsync(int count)
        {
            try
            {
                if ((Connected && Stream != null) && Status == EntityState.None)
                {
                    if (Stream.CanRead && Stream.DataAvailable)
                    {
                        Status = EntityState.ReadMode;

                        byte[] buffer = new byte[count];

                        int read = await Stream.ReadAsync(buffer);

                        if (buffer.Length < read) buffer = buffer.Take(read).ToArray();

                        Status = EntityState.None;

                        return buffer;
                    }
                }

                return null;
            } 
            catch
            {
                Dispose();

                return null;
            }
        }

        /// <summary>
        /// Write Bytes To Buffer And Change State
        /// </summary>
        /// <param name="buffer">Bytes Buffer To Write</param>
        /// <returns>True If Wrote, Otherwise False</returns>
        public async ValueTask<bool> WriteAsync(byte[] buffer)
        {
            try
            {
                if ((Connected && Stream != null) && Status == EntityState.None)
                {
                    if (Stream.CanWrite && buffer != null)
                    {
                        Status = EntityState.WriteMode;

                        await Stream.WriteAsync(buffer);
                        await Stream.FlushAsync();

                        Status = EntityState.None;

                        return true;
                    }
                }

                return false;
            } 
            catch
            {
                Dispose();

                return false;
            }
        }

        /// <summary>
        /// Free Memory
        /// </summary>
        public void Dispose()
        {
            Entity?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
