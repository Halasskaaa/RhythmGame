namespace wah.Chart.SSC
{
	internal readonly record struct RadarValues(float Stream, float Voltage, float Air, float Freeze, float Chaos)
	{
		public static bool TryParse(ReadOnlySpan<char> src, out RadarValues value)
		{
			value = default;

			Span<float> values = stackalloc float[5];
			byte i = 0;
			foreach (var range in src.Split(','))
			{
				if (!float.TryParse(src[range].Trim(), out var result)) return false;
				values[i++] = result;
			}

			if (i != values.Length) return false;

			value = new RadarValues(values[0], values[1], values[2], values[3], values[4]);
			return true;
		}
	}
}
