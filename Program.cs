using System;
using SDL3;
using System.Diagnostics;
using wah;
using wah.Scenes;

internal class Program
{
	[STAThread]
	public static int Main(string[] args)
	{
		if (!SDL.Init(SDL.InitFlags.Audio | SDL.InitFlags.Video | SDL.InitFlags.Events))
		{
			SDL.LogError(SDL.LogCategory.System, $"initialization failed: {SDL.GetError()}");
			return -1;
		}

		if (!Mixer.Init())
		{
			SDL.LogError(SDL.LogCategory.Audio, $"audio mixer initialization failed: {SDL.GetError()}");
			SDL.Quit();
			return -1;
		}

		if (!TTF.Init())
		{
			SDL.LogError(SDL.LogCategory.Render, $"ttf initialization failed: {SDL.GetError()}");
			Mixer.Quit();
			SDL.Quit();
			return -1;
		}

		try
		{
			var mainWindow = new Window("petty game");
			var renderer = mainWindow.Renderer;

			var clearColor = new SDL.FColor(100 / 255f, 149 / 255f, 237 / 255f, 0);

			renderer.FDrawColor = clearColor;

			var prevTS = Stopwatch.GetTimestamp();
			var loop = true;
			SceneManager.Current = new TitleScreen();

			while (loop)
			{
				while (SDL.PollEvent(out var e))
				{
					switch((SDL.EventType)e.Type)
					{
						case SDL.EventType.Quit:
							loop = false;
							break;
							case SDL.EventType.KeyDown:
							case SDL.EventType.KeyUp:
							SceneManager.Current.OnInput(new InputEvent(e.Key));
							break;
							case SDL.EventType.MouseButtonDown:
							case SDL.EventType.MouseButtonUp:
							SceneManager.Current.OnInput(new InputEvent(e.Button));
							break;
					}
				}

				var current = Stopwatch.GetTimestamp();
				var delta = Stopwatch.GetElapsedTime(prevTS);
				prevTS = current;

				renderer.FDrawColor = clearColor;
				renderer.Clear();
                SceneManager.Current.OnDrawFrame(delta, ref renderer);
                renderer.Present();
			}
		}
		finally
		{
			TTF.Quit();
			Mixer.Quit();
			SDL.Quit();
		}

		return 0;
	}
}