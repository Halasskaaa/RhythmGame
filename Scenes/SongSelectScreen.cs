using SDL3;
using wah.Chart.SSC;

namespace wah.Scenes
{
	internal class SongSelectScreen : IScene
	{
		const ushort NoSelection = ushort.MaxValue;

		private static readonly DirectoryInfo PacksRoot = new("../../../test");

		private ushort selectedPackIdx, selectedSongIdx;
		private UIAction action;

		// TODO: actually list charts
		public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
		{
			//Load(new ChartPreview
			//{
			//    ChartPath = new("../../../test/DDDimocratic AAAnnihilation//DDDimocratic AAAnnihilation//Idol.ssc"),
			//    AudioPath = new("../../../test/DDDimocratic AAAnnihilation//DDDimocratic AAAnnihilation//Idol.ogg"),
			//    PreviewStart = 0,
			//    PreviewEnd = 1,
			//});

			// test
			//SSCSimfile simfile;
			//var ok = new SSCParser(File.ReadAllText("../../../test/DDDimocratic AAAnnihilation/DDDimocratic AAAnnihilation/Idol/Idol.ssc"),
			//                       new DirectoryInfo("../../../test/DDDimocratic AAAnnihilation/DDDimocratic AAAnnihilation/Idol"))
			//   .Parse(out simfile);

			//Debug.Assert(ok, "failed to parse chart");


			//SceneManager.Current = new PlayerScene(simfile, 0);

			const float itemHeight = 100;
			const float itemWidth = 400;
			const float subItemWidth = 360;

			var itemColor = new SDL.FColor(0.6f, 0.6f, 0.6f, 1.0f);
			var selectedItemColor = new SDL.FColor(0.8f, 0.8f, 0.8f, 1.0f);
			var textColor = new SDL.FColor(1.0f, 1.0f, 1.0f, 1.0f);

			var area = renderer.RenderArea;
			var i = ushort.MinValue;
			var j = ushort.MinValue;
			var y = 0f;

			var packRect = new SDL.FRect { X = area.W - itemWidth, Y = y, W = itemWidth, H = itemHeight };
			var songRect = new SDL.FRect { X = area.W - subItemWidth, Y = y, W = subItemWidth, H = itemHeight };

			foreach (var pack in PacksRoot.EnumerateDirectories())
			{

				renderer.DrawRectFilled(packRect with { Y = y }, i == selectedPackIdx ? selectedItemColor : itemColor);
				renderer.DrawText(pack.Name, packRect.X, y, textColor);

				y += packRect.H;

				if (i == selectedPackIdx)

				{
					foreach (var song in pack.EnumerateDirectories())
					{
						renderer.DrawRectFilled(songRect with { Y = y }, j == selectedSongIdx ? selectedItemColor : itemColor);
						renderer.DrawText(song.Name, songRect.X, y, textColor);

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

			switch (action)
			{
				case UIAction.Select:
					if (selectedSongIdx != NoSelection)
					{
						var songRoot = PacksRoot.EnumerateDirectories().ElementAt(i).EnumerateDirectories().ElementAt(j);
						if (songRoot.EnumerateFiles("*.ssc").FirstOrDefault() is not { } sscFile) break;
						using var reader = sscFile.OpenText();
						if (new SSCParser(reader.ReadToEnd(), songRoot).Parse(out var simfile) || simfile.Charts?.Length == 0) SceneManager.Current = new PlayerScene(simfile, 0);
					}
					else
					{
						selectedSongIdx = 0;
					}
					break;
				case UIAction.Down:
					if (selectedSongIdx == NoSelection)
					{
						selectedPackIdx++;
						if (selectedPackIdx > i) selectedPackIdx = 0;
					}
					else
					{
						selectedSongIdx++;
						if (selectedSongIdx > j) selectedSongIdx = 0;
					}
					break;
				case UIAction.Up:
					if (selectedSongIdx == NoSelection)
					{
						selectedPackIdx--;
						if (selectedPackIdx == ushort.MaxValue) selectedPackIdx = i;
					}
					else
					{
						selectedSongIdx--;
						if (selectedSongIdx > j) selectedSongIdx = NoSelection;
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
