namespace wah.Chart.SSC
{
	internal readonly record struct Delay(float StartBeat, float DurationSeconds)
	{
		public static bool TryParse(ReadOnlySpan<char> src, out Delay delay) {
			delay = default;
			src = src.Trim();
			Span<Range> splits = stackalloc Range[2];
			if (src.Split(splits, '=') != splits.Length) return false;

			if (!float.TryParse(src[splits[0]], out var start) || !float.TryParse(src[splits[1]], out var duration)) return false;

			delay = new Delay(start, duration);
			return true;
		}
	}
}
