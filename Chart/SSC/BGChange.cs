using System;
using System.IO;
using System.Runtime.CompilerServices;
using SDL3;
using wah.Util;

namespace wah.Chart.SSC
{
	// Specifies what the background of a song changes to throughout the chart. This allows lua files to be loaded.
	// The FGCHANGES line uses the same format, except only the start beat and first file are used (values 1 and 2).
	internal readonly record struct BGChange(BGChange.Layer OnLayer,
		float Beat,
		BGChange.BGSrc BG,
		bool CrossFade,
		bool StretchRewind,
		bool StretcNoLoop,
		string? EffectName, // BackgroundEffects folder will be searched for a match
		string? File2Name,
		string? TransitionName, //  The BackgroundTransitions folder will be searched for a match
		SDL.Color Color,
		SDL.Color Color2)
	{
		public enum Layer
		{
			BG1,
			BG2,
			BG3,
			FG1, // foreground changes use the same format
		}

		// does not include the layer, that has to be set manually
		public static bool TryParse(ReadOnlySpan<char> src, DirectoryInfo chartRoot, out BGChange BGChange)
		{
			// beat = file_or_folder = update_rate = crossfade = stretchrewind = stretchnoloop = Effect = File2 = Transition = Color1 = Color2
			// beat: The beat this BGCHANGE occurs on. Can be negative to start before the first beat.
			// file_or_folder: The relative path to the file to use for the BGCHANGE. Lua files are allowed.If a folder is given, it looks for "default.lua".
			// update_rate: The update rate of the BGCHANGE.
			// crossfade: set to 1 if using a crossfade. Overridden by Effect.
			// stretchrewind: set to 1 if using stretchrewind.Overridden by Effect.
			// stretchnoloop: set to 1 if using stretchnoloop.Overridden by Effect.
			// Effect: What BackgroundEffect to use.
			// File2: The second file to load for this BGCHANGE.
			// Transition: How the background transitions to this.
			// Color1 / Color2: Formatted as red ^ green ^ blue ^ alpha, with the values being from 1 to 0, Passed to the BackgroundEffect with the LuaThreadVariable "Color1" / "Color2" in web hexadecimal format as a string.Alpha is optional.
			// Often, a last entry with "-nosongbg-" as the file is placed so the song's starting background doesn't show up at the end.

			BGChange = default;

			Span<Range> splits = stackalloc Range[11];
			if (src.Split(splits,'=') != splits.Length) return false;

			if (!float.TryParse(src[splits[0]], out var beat)) return false;

			if (src[splits[1]].Length + chartRoot.FullName.Length > PathBuffer.Length) return false;
			var fullPath = chartRoot.AppendPath(src[splits[1]]);
			var buff = new PathBuffer();
			src[splits[1]].CopyTo(buff);
			var bgSrc = new BGSrc(buff, src[splits[1]] switch
			{
				"-nosongbg-" => BGSrc.BGType.SongBackground,
				"-songbackground-" => BGSrc.BGType.SongBackground,
				var @default => (File.GetAttributes(src[splits[1]].ToString()) & FileAttributes.Directory) != 0 ? BGSrc.BGType.Lua : BGSrc.BGType.Image,
			});

			return true;
		}

		public readonly record struct BGSrc(PathBuffer Path, BGSrc.BGType Type)
		{
			public enum BGType
			{
				Image, // file path
				Lua, // folder path
				NoSongBG, // value is "-nosongbg-"
				SongBackground, // value is "-songbackground-"
			}
		}

		[InlineArray(Length)]
		public struct PathBuffer
		{
			public const byte Length = 255;
			char value;
		}
	}
}
