using SDL3;
using wah.Chart.SSC;

namespace wah.Scenes
{
	internal class SongSelectScreen : IScene
	{
		const ushort NoSelection = ushort.MaxValue;

		private static readonly DirectoryInfo PacksRoot = new("../../../test");


		private ushort selectedPackIdx, _selectedSongIdx = NoSelection;
		private ushort SelectedSongIdx
		{
			get => _selectedSongIdx; set
			{
				_selectedSongIdx = value;

				if (value == NoSelection) return;
				if (PacksRoot.EnumerateDirectories()
					.ElementAt(selectedPackIdx)
					.EnumerateDirectories()
					.ElementAtOrDefault(SelectedSongIdx) is not { } songRoot) return;
				if (songRoot.EnumerateFiles("*.ssc").FirstOrDefault() is not { } sscFile) return;

				previewAudio?.Stop();

				using var reader = sscFile.OpenText();
				if (new SSCParser(reader.ReadToEnd(), songRoot).Parse(out var simfile) && simfile.Music is not null)
					previewAudio = new Audio.Audio(simfile.Music);

				if (previewAudio is null) return;

				previewAudio.PlayBackPosition = TimeSpan.FromSeconds(simfile.SampleStart);
				previewAudio.Play();
			}
		}
		private UIAction action;
		private float scroll;
		Audio.Audio? previewAudio;

		public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
		{
			const float itemHeight = 100;
			const float itemWidth = 400;
			const float subItemWidth = 360;

			var itemColor = new SDL.FColor(0.6f, 0.6f, 0.6f, 1.0f);
			var selectedItemColor = new SDL.FColor(0.8f, 0.8f, 0.8f, 1.0f);
			var textColor = new SDL.FColor(1.0f, 1.0f, 1.0f, 1.0f);

			var area = renderer.RenderArea;
			var i = ushort.MinValue;
			var j = ushort.MinValue;
			var y = scroll;
			var selectedY = 0f;

			var packRect = new SDL.FRect { X = area.W - itemWidth, Y = y, W = itemWidth, H = itemHeight };
			var songRect = new SDL.FRect { X = area.W - subItemWidth, Y = y, W = subItemWidth, H = itemHeight };

			foreach (var pack in PacksRoot.EnumerateDirectories())
			{

				renderer.DrawRectFilled(packRect with { Y = y }, i == selectedPackIdx ? selectedItemColor : itemColor);
				renderer.DrawTextCentered(pack.Name, packRect.X + packRect.W / 2f, y + packRect.H / 2f, textColor);

				if (i == selectedPackIdx) selectedY = y;

				y += packRect.H;

				if (i == selectedPackIdx)
				{
					foreach (var song in pack.EnumerateDirectories())
					{
						renderer.DrawRectFilled(songRect with { Y = y }, j == SelectedSongIdx ? selectedItemColor : itemColor);
						renderer.DrawTextCentered(song.Name, songRect.X + songRect.W / 2f, y + songRect.H / 2f, textColor);

						if (j == SelectedSongIdx) selectedY = y;

						y += songRect.H;
						j++;
					}

					// j is max idx + 1 AKA song count
					j--;
				}
				i++;
			}

			// i is max idx + 1 AKA pack count
			i--;

			if (selectedY < area.Y) scroll += itemHeight;
			if (selectedY > area.Y + area.H) scroll -= itemHeight;

			switch (action)
			{
				case UIAction.Select:
					if (SelectedSongIdx != NoSelection)
					{
						var songRoot = PacksRoot.EnumerateDirectories().ElementAt(selectedPackIdx).EnumerateDirectories().ElementAt(SelectedSongIdx);
						if (songRoot.EnumerateFiles("*.ssc").FirstOrDefault() is not { } sscFile) break;
						using var reader = sscFile.OpenText();

						previewAudio?.Pause();

						if (new SSCParser(reader.ReadToEnd(), songRoot).Parse(out var simfile) || simfile.Charts?.Length == 0) SceneManager.Current = new PlayerScene(simfile, (uint)(simfile.Charts.Length - 1));
					}
					else
					{
						SelectedSongIdx = 0;
					}
					break;
				case UIAction.Down:
					if (SelectedSongIdx == NoSelection)
					{
						selectedPackIdx++;
						if (selectedPackIdx > i) selectedPackIdx = 0;
					}
					else
					{
						SelectedSongIdx++;
						if (SelectedSongIdx > j) SelectedSongIdx = NoSelection;
					}
					break;
				case UIAction.Up:
					if (SelectedSongIdx == NoSelection)
					{
						selectedPackIdx--;
						if (selectedPackIdx == ushort.MaxValue) selectedPackIdx = i;
					}
					else
					{
						SelectedSongIdx--;
						if (SelectedSongIdx > j) SelectedSongIdx = NoSelection;
					}
					break;
				default: break;
			}

			action = UIAction.None;
		}

		public void OnInput(in InputEvent input)
		{
			if (input.Type != InputEvent.EventType.Key) return;

			var k = input.Key;

			if (!k.Down) return;

			action = k.Key switch
			{
				SDL.Keycode.Return => UIAction.Select,
				SDL.Keycode.Down => UIAction.Down,
				SDL.Keycode.Up => UIAction.Up,
				_ => UIAction.None
			};

			if (k.Key == SDL.Keycode.Escape) SceneManager.Current = new TitleScreen();
		}

		private enum UIAction
		{
			None,
			Select,
			Down,
			Up
		}
	}
}
