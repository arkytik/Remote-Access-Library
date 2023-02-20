using Microsoft.Razor.RPA.Common.Handlers;

namespace Microsoft.Razor.RPA.Plugins
{
    /// <summary>
    /// Provides Functions To Load Plugins Into Tcp Handler
    /// </summary>
    public class PluginLoader
    {
        public IHandler Handler { get; protected set; }
        public List<IPlugin> Plugins { get; protected set; } = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handler">Tcp Handler</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PluginLoader(IHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Handler = handler;
        }

        /// <summary>
        /// Load Plugins
        /// </summary>
        public virtual async void Load() => await LoadAsync();

        /// <summary>
        /// Load Plugins Async
        /// </summary>
        /// <returns></returns>
        public virtual async Task LoadAsync()
        {
            Task t = Task.Run(() =>
            {
                Parallel.ForEach(Plugins, plugin =>
                {
                    if (Handler != null)
                    {
                        plugin.Handler = Handler;

                        Handler.OnReceiveHandler += (content, clientId) =>
                        {
                            try
                            {
                                if (Handler == null || content.IsNullOrEmpty()) return;

                                plugin.Handle(content, clientId);

                                content.Reset();
                            }
                            catch { }
                        };
                    }
                });
            });

            await t;
        }

        /// <summary>
        /// Add Plugin
        /// </summary>
        /// <param name="plugin"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void AddPlugin(IPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            if (!Plugins.Contains(plugin)) Plugins.Add(plugin);
        }

        /// <summary>
        /// Remove Plugin
        /// </summary>
        /// <param name="plugin"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void RemovePlugin(IPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            if (Plugins.Contains(plugin)) Plugins.Remove(plugin);
        }
    }
}
