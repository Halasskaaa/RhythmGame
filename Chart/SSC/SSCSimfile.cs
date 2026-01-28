namespace wah.Chart.SSC
{
	internal readonly record struct SSCSimfile(SCCVersion Version,
		SSCSimfile.StringWithTranslit Title,
		SSCSimfile.StringWithTranslit Subtitle,
		SSCSimfile.StringWithTranslit Artist,
		string Origin,
		string Genre,
		string Credit,
		FileInfo? Banner,
		FileInfo? Background,
		FileInfo? LyricsPath,
		FileInfo? CDTitle,
		FileInfo? Music,
		InstrumentTrack InstrumentTrack,
		float MusicLength,      // cache
		float Offset,           // in seconds
		BPMChange[] BPMs,
		float[] Stops,          // not correct; Alias: freezes
		Delay[] Delays,         // Specifies the Delay segments for the song.
		string[] Labels,        // not correct
		TimeSignatureChange[] TimeSignatures,
		// long[] TickCounts,   // only used in PIU mode (which we won't support)
		float SampleStart,      // in seconds
		float SampleLength,     // in seconds
		string DisplayBPM,      // [xxx][xxx:xxx]|[*]
								// "*" means it's random, and constantly changing characters are shown for BPM.
								// If not set, the song's min and max BPM are shown instead.
		bool Selectable,        // Sets whether the song is shown in the music wheel or not.
								// This is ignored if the HiddenSongs preference is turned off.
		string LastSecondHint,
		BGChange[] BGChanges,   // Alias: animations
		BGChange[] FGChanges,
		FileInfo[] KeySounds,
		Attack[] Attacks,       // scripted modifiers
		string PreviewID,       // probably incorrect
		string Jacket,          // probably a path
		string DiscImage,       // probably a path
		string Preview,         // probably a preview struct
		string[] Warps,         // it's own struct
		string[] Combos,        // incorrect
		float[] Speeds,         // incorrect
		float[] Scrolls,        // incorrect
		float[] Fakes,          // incorrect
		SSCChart[] Charts
		)
	{
		public readonly record struct StringWithTranslit(string Value, string Translit);
	}
}
