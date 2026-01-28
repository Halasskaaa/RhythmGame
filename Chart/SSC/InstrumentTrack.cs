using System;
using System.IO;
using wah.Util;

namespace wah.Chart.SSC
{
	internal readonly record struct InstrumentTrack(FileInfo Path, InstrumentTrack.InstrumentType Instrument)
	{
		public enum InstrumentType : byte
		{
			Guitar,
			Rhythm,
			Bass,
			Vocal,
			Drum1,
			Drum2,
			Drum3,
			Drum4
		}

		public static bool TryParse(ReadOnlySpan<char> src, DirectoryInfo chartRoot, out InstrumentTrack track)
		{
			track = default;
			var split = src.IndexOf('=');
			if (split == -1 || split == src.Length - 1) return false;

			var instrumentSrc = src[(split + 1)..];
			Span<char> instrumentSrcLower = stackalloc char[instrumentSrc.Length];
			instrumentSrc.ToLowerInvariant(instrumentSrcLower);
			switch (instrumentSrcLower)
			{
				case "guitar": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Guitar); return true;
				case "rhythm": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Rhythm); return true;
				case "bass": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Bass); return true;
				case "vocal": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Vocal); return true;
				case "drum1": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Drum1); return true;
				case "drum2": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Drum2); return true;
				case "drum3": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Drum3); return true;
				case "drum4": track = new(chartRoot.AppendPath(src[..split]), InstrumentType.Drum4); return true;
				default: return false;
			}
		}
	}
}
