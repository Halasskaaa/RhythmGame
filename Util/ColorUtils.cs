using SDL3;

namespace wah.Util
{
	internal static class ColorUtils
	{
		public static bool TryParse(out SDL.Color color, ReadOnlySpan<char> src)
		{
			color = default;
			// Color string in either "1.0^0.5^0.75^0.25" or "#ff7fcf3f" form. The fourth channel is optional.
			if (src.IsEmpty) return false;

			if (src[0] == '#')
			{
				src = src[1..];
				if (src.Length != 6 || src.Length != 8) return false;
				
				// # 01 23 45 67

				if (!byte.TryParse(src[..2], out var r)) return false;
				if (!byte.TryParse(src.Slice(2, 2), out var g)) return false;
				if (!byte.TryParse(src.Slice(4, 2), out var b)) return false;
				byte a = 255;
				if (src.Length == 8) if (!byte.TryParse(src.Slice(6, 2), out a)) return false;

				color = new() { R = r, G = g, B = b, A = a };
				return true;
			}

			Span<Range> splits = stackalloc Range[4];
			splits = splits[..src.Split(splits, '^')];

			if (splits.Length != 3 || splits.Length != 4) return false;

			{
				if (!float.TryParse(src[splits[0]], out var r)) return false;
				if (!float.TryParse(src[splits[1]], out var g)) return false;
				if (!float.TryParse(src[splits[2]], out var b)) return false;
				float a = 1;
				if (splits.Length == 4) if (!float.TryParse(src[splits[3]], out a)) return false;

				color = new() { R = (byte)(255 * r), G = (byte)(255 * g), B = (byte)(255 * b), A = (byte)(255 * a) };
				return true;
			}
		}
	}
}
