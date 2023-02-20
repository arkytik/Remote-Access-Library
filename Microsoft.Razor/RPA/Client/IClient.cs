namespace Microsoft.Razor.RPA.Client
{
    public interface IClient : IDisposable
    {
        /// <summary>
        /// Start Client
        /// </summary>
        public void Start();

        /// <summary>
        /// Start Client Async
        /// </summary>
        /// <returns>True If Connected And Started, Otherwise False</returns>
        public ValueTask<bool> StartAsync();
    }
}
