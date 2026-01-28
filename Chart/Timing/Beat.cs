namespace wah.Chart.Timing
{
	internal readonly record struct Beat(ushort Numerator, ushort Denominator)
	{
		public float Value => (float)Numerator / (float)Denominator;
	}
}
