using SDL3;

internal class TextRenderer
{
    private readonly IntPtr       font;
    private readonly IntPtr       textEngine;
    
    public TextRenderer(FileInfo fontFile, float fontSize, IntPtr renderer)
    {
        font = TTF.OpenFont(fontFile.FullName, fontSize);
        if (font == IntPtr.Zero)
        {
            SDL.LogError(SDL.LogCategory.Application,
                         $"failed to load font from \"{fontFile.FullName}\": {SDL.GetError()}");
            return;
        }

        textEngine = TTF.CreateRendererTextEngine(renderer);
        if (textEngine == IntPtr.Zero)
            SDL.LogError(SDL.LogCategory.Application, $"failed to create text engine: {SDL.GetError()}");
    }

    public void RenderText(in string text, float x, float y)
    {
        var textObject = TTF.CreateText(textEngine, font, text, (nuint)text.Length);
        if (textObject == IntPtr.Zero)
        {
            SDL.LogError(SDL.LogCategory.Application, $"failed to create text object: {SDL.GetError()}");
            return;
        }

        if (!TTF.DrawRendererText(textObject, x, y))
        {
            SDL.LogError(SDL.LogCategory.Render, $"failed to render text object: {SDL.GetError()}");
        }
        
        TTF.DestroyText(textObject);
    }

    public void RenderTextCentered(in string text, float x, float y)
    {
		var textObject = TTF.CreateText(textEngine, font, text, (nuint)text.Length);
		if (textObject == IntPtr.Zero)
		{
			SDL.LogError(SDL.LogCategory.Application, $"failed to create text object: {SDL.GetError()}");
			return;
		}

        if (!TTF.GetTextSize(textObject, out var width, out var height))
        {
			SDL.LogError(SDL.LogCategory.Application, $"failed to get text size: {SDL.GetError()}");
		}

        x -= width / 2f;
        y -= height / 2f;

		if (!TTF.DrawRendererText(textObject, x, y))
		{
			SDL.LogError(SDL.LogCategory.Render, $"failed to render text object: {SDL.GetError()}");
		}

		TTF.DestroyText(textObject);
	}

    ~TextRenderer()
    {
        if (textEngine != IntPtr.Zero)
            TTF.DestroyRendererTextEngine(textEngine);
        if (font != IntPtr.Zero)
            TTF.CloseFont(font);
    }
}
