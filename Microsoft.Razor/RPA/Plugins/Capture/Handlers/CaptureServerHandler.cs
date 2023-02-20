using Microsoft.Razor.RPA.Common.Entity;
using Microsoft.Razor.RPA.Common.Message;
using Microsoft.Razor.RPA.Common.Handlers;
using Windows.Media.Playback;

namespace Microsoft.Razor.RPA.Plugins.Capture.Handlers
{
    /// <summary>
    /// Provides Functions For Handling Messages
    /// </summary>
    public class CaptureServerHandler : IPlugin
    {
        //  Plugin Data
        public int Version => 1;

        public string Name => "Capture-Server";

        //  Other
        public const int MinDelay = 1;

        private int _delay = 10;
        private bool _isRun = false;
        
        public int Delay { get => _delay > 0 ? _delay : MinDelay; protected set => _delay = value > 0 ? value : MinDelay; }

        public bool IsRun { 
            get => _isRun; 
            protected set
            {
                _isRun = value;

                if (IsRun == true) Start();
            }
        }

        public IHandler? Handler { get; set; }

        public ContentMessage ContentCache { get; protected set; }

        public List<int> Viewers { get; protected set; } = new();

        /// <summary>
        /// Constructor Of Capture Server Handler
        /// </summary>
        /// <param name="delay">Screnshot Update Delay In MS</param>
        public CaptureServerHandler(int delay = 10)
        {
            Delay = delay;
            IsRun = false;

            ContentCache = new();
        }

        /// <summary>
        /// Start Capture Server Handler
        /// </summary>
        public virtual async void Start() => await StartAsync();

        /// <summary>
        /// Start Capture Server Handler Async
        /// </summary>
        /// <returns></returns>
        public virtual async Task StartAsync()
        {
            if (Handler == null) return;

            ContentCache = new(ContentType.Image, ContentFrom.Server);

            while (IsRun)
            {
                if (Viewers.Count > 0)
                {
                    ContentCache.Data = ScreenCapture.CaptureScreenToBytesInJpeg();
                    ContentCache.Size = ContentCache.Data.Length;

                    Parallel.ForEach(Viewers, async viewerId =>
                    {
                        if (!ContentCache.IsNullOrEmpty())
                        {
                            TCPEntity? viewer = Handler.Clients[viewerId];

                            if (viewer != null)
                            {
                                bool isWrote = await ContentCache.WriteAsync(Handler, viewer);

                                if (!isWrote)  // Try Again
                                {
                                    isWrote = await ContentCache.WriteAsync(Handler, viewer);

                                    if (!isWrote) RemoveViewer(viewerId);
                                }
                            }
                            else RemoveViewer(viewerId);
                        }
                        else RemoveViewer(viewerId);
                    });
                }
                else if (IsRun)
                {
                    IsRun = false;

                    ContentCache.Reset();
                }

                await Task.Delay(Delay);
            }

            ContentCache.Reset();
        }

        /// <summary>
        /// Handle Requests
        /// </summary>
        /// <param name="content">Client Message</param>
        /// <param name="clientId">Tcp Client Id</param>
        public virtual async void Handle(ContentMessage content, int clientId) => await HandleAsync(content, clientId);

        /// <summary>
        /// Handle Requests Async
        /// </summary>
        /// <param name="content">Client Message</param>
        /// <param name="clientId">Tcp Client Id</param>
        /// <returns></returns>
        public virtual async Task HandleAsync(ContentMessage content, int clientId)
        {
            Task t = Task.Run(() =>
            {
                if (content.From == ContentFrom.Client && content.Type == ContentType.Request)
                {
                    RequestType? request = content.GetContent<RequestType>();

                    if (request != null)
                    {
                        switch (request)
                        {
                            case RequestType.StartVideoStream:
                                AddViewer(clientId);
                                break;

                            case RequestType.StopVideoStream:
                                RemoveViewer(clientId);
                                break;
                        }
                    }
                }
            });

            await t;
        }

        /// <summary>
        /// Add New Viewer Of Video Stream
        /// </summary>
        /// <param name="viewerId">Id Of Viewer</param>
        public void AddViewer(int viewerId)
        {
            if (Handler == null) return;

            if (viewerId < 0 || viewerId > Handler.Clients.Count) return;

            TCPEntity? client = Handler.Clients.ElementAtOrDefault(viewerId);

            if (client != null && client.Connected)
            {
                if (!Viewers.Contains(viewerId))
                {
                    Viewers.Add(viewerId);

                    if (!IsRun) IsRun = true;
                }

                client = null;
            }
        }

        /// <summary>
        /// Remove Viewer Of Video Stream
        /// </summary>
        /// <param name="viewerId">Id Of Viewer</param>
        public void RemoveViewer(int viewerId)
        {
            if (Handler == null) return;

            if (viewerId < 0 || viewerId > Viewers.Count) return;

            if (Viewers.Contains(viewerId))
            {
                Viewers.Remove(viewerId);

                if (Viewers == null || Viewers.Count == 0) IsRun = false;
            }
        }
    }
}
