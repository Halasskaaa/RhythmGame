namespace wah.Chart.SSC
{
	internal readonly record struct BPMChange(float FromBeat, float BPM)
	{
		public static bool TryParse(ReadOnlySpan<char> src, out BPMChange bpmChange) {
			bpmChange = default;
			
			src = src.Trim();
			Span<Range> splits = stackalloc Range[2];
			if (src.Split(splits, '=') != splits.Length) return false;
			if (!float.TryParse(src[splits[0]], out var beat) || !float.TryParse(src[splits[1]], out var bpm)) return false;

			bpmChange = new(beat, bpm);
			return true;
		}
	}
}
