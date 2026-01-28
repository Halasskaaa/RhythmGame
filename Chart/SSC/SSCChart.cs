namespace wah.Chart.SSC
{
	internal readonly record struct SSCChart(StepsType StepsType,
		string Description,
		string Difficulty, // I assume they meant difficulty name
		float Meter, // difficulty rating
		float Offset,
		RadarValues RadarValues,
		// SSCNote[] Notes, //alias: NOTES2
		SSCMeasureEntry[] Notes, // alias: NOTES2
		string ChartName,
		string Credit
		);
}
