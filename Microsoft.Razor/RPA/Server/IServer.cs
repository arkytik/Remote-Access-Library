namespace Microsoft.Razor.RPA.Server
{
    public interface IServer : IDisposable
    {
        /// <summary>
        /// Start Server
        /// </summary>
        public void Start();

        /// <summary>
        /// Start Server Async
        /// </summary>
        /// <returns></returns>
        public Task StartAsync();
    }
}
