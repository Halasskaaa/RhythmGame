using System.IO;
using SDL3;

namespace wah
{
    internal readonly struct WindowRenderer(Window window, nint windowRenderer)
    {
        private readonly TextRenderer textRenderer = new(new FileInfo("../../../Assets/PublicPixel.ttf"), 12, windowRenderer);
        
        public SDL.FColor FDrawColor
        {
            get
            {
                if (!SDL.GetRenderDrawColorFloat(windowRenderer, out var r, out var g, out var b, out var a)) SDL.LogError(SDL.LogCategory.Render, $"failed to get fdraw color: {SDL.GetError()}");
                return new SDL.FColor(r, g, b, a);
            }
            set
            {
                if (!SDL.SetRenderDrawColorFloat(windowRenderer, value.R, value.G, value.B, value.A)) SDL.LogError(SDL.LogCategory.Render, $"failed set fdraw color: {SDL.GetError()}");
            }
        }

        public SDL.FRect RenderArea
        {
            get
            {
                var pxs = window.PixelSize;
                return new() { X = 0, Y = 0, W = pxs.X, H = pxs.Y };
            }
        }
        
        public void Clear()
        {
            if (!SDL.RenderClear(windowRenderer)) SDL.LogError(SDL.LogCategory.Render, $"failed to clear: {SDL.GetError()}");
        }

        public void Present()
        {
            if (!SDL.RenderPresent(windowRenderer)) SDL.LogError(SDL.LogCategory.Render, $"failed to present: {SDL.GetError()}");
        }

        public void DrawRectFilled(in SDL.FRect rect, in SDL.FColor color)
        {
            var prev = FDrawColor;
            FDrawColor = color;
            if (!SDL.RenderFillRect(windowRenderer, in rect)) SDL.LogError(SDL.LogCategory.Render, $"failed to draw rect: {SDL.GetError()}"); 
            FDrawColor = prev;
        }

        public void DrawText(in string text, float x, float y)
        {
            textRenderer.RenderText(in text, x, y);
        }

        public void DrawText(in string text, in float x, in float y, SDL.FColor color)
        {
            var prev = FDrawColor;
            FDrawColor = color;
            DrawText(in text, x, y);
            FDrawColor = prev;
        }
    }
}