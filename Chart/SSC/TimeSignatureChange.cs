namespace wah.Chart.SSC
{
	internal readonly struct TimeSignatureChange(float FromBeat, byte Numerator, byte Denominator)
	{
		public static bool TryParse(ReadOnlySpan<char> src, out TimeSignatureChange result)
		{
			result = default;
			src = src.Trim();

			Span<Range> splits = stackalloc Range[3];
			if (src.Split(splits, '=') != splits.Length) return false;

			if (!(float.TryParse(src[splits[0]], out var beat)
				&& byte.TryParse(src[splits[1]], out var numerator)
				&& byte.TryParse(src[splits[2]], out var denominator))) return false;

			result = new TimeSignatureChange(beat, numerator, denominator);
			return true;
		}
	}
}
