using Microsoft.Razor.RPA.Common.Entity;

namespace Microsoft.Razor.RPA.Common.List
{
    /// <summary>
    /// Modify List With TCP Clients Functions
    /// </summary>
    public class ClientsList : List<TCPEntity>, IDisposable
    {
        public Action<TCPEntity>? OnAdded;
        public Action<TCPEntity>? OnRemoved;

        /// <summary>
        /// Ovveride Get Item By Index
        /// </summary>
        /// <param name="index">Index Of TcpEntity</param>
        /// <returns></returns>
        public new TCPEntity? this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                {
                    if (base[index].Connected) return base[index];

                    Remove(base[index]);
                }

                return null;
            }
        }

        /// <summary>
        /// Modify Add Function With Invoke Event
        /// </summary>
        /// <param name="client">Tcp Client</param>
        public new void Add(TCPEntity client)
        {
            if (client == null) return;

            if (client.Connected)
            {
                base.Add(client);

                OnAdded?.Invoke(client);
            }
        }

        /// <summary>
        /// Modify Remove Function With Invoke Event
        /// </summary>
        /// <param name="client">Tcp Client</param>
        public new void Remove(TCPEntity client)
        {
            if (client == null) return;

            if (Contains(client))
            {
                base.Remove(client);

                OnRemoved?.Invoke(client);

                //  Free
                client?.Dispose();
            }
        }

        /// <summary>
        /// Dispose All Tcp Clients And Clear List
        /// </summary>
        public void Dispose()
        {
            try
            {
                foreach (TCPEntity client in this) client?.Dispose();

                Clear();
            } catch { }

            GC.SuppressFinalize(this);
        }
    }
}
