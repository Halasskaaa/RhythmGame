using SDL3;

namespace wah
{
    internal class Window
    {
        readonly private nint m_Window, m_Renderer;
        readonly private bool m_Initialized;
        private readonly int m_Width, m_Height;

        public Window(in string title)
        {
            // FIXME: we are assuming that both window and render creation either fails or succeeds
            if (!SDL.CreateWindowAndRenderer(title, 0, 0, SDL.WindowFlags.Fullscreen, out m_Window, out m_Renderer))
            {
                SDL.LogError(SDL.LogCategory.Application, $"error creating window and rendering: {SDL.GetError()}");
                return;
            }

            m_Initialized = true;
            if (!SDL.GetWindowSizeInPixels(m_Window, out m_Width, out m_Height))
                SDL.LogError(SDL.LogCategory.Application, $"failed to get window size in pixels {SDL.GetError()}");
        }

        public bool Initialized => m_Initialized;

        public WindowRenderer Renderer => m_Initialized ? new(this, m_Renderer) : throw new InvalidOperationException("window is not initialized");
        public SDL.Point PixelSize => new() { X = m_Width, Y = m_Height };

        ~Window()
        {
            if (!m_Initialized) return;
            SDL.DestroyRenderer(m_Renderer);
            SDL.DestroyWindow(m_Window);
        }
    }
}
