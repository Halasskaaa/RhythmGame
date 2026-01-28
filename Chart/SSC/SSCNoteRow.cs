using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using wah.Chart.Timing;

namespace wah.Chart.SSC;

[InlineArray(Length)]
internal struct SSCNoteRow
{
    public const int Length = 4;
    SSCNoteType _;
}

internal enum SSCNoteType : byte
{
    Empty,
    Tap,
    HoldHead,
    Tail,
    RollHead,
    Mine,
    Lift,
    Fake
}

// we enter the next measure when numerator/denominator decreases
internal struct SSCMeasureEntry
{
    public SSCNoteRow row;
    public ushort     measure;
    public ushort     numerator;
    public ushort     denominator;
    public Beat       beat;
}

internal ref struct SSCSteps(ReadOnlySpan<char> src, List<SSCMeasureEntry> entries)
{
    ReadOnlySpan<char> src = src;
    SSCMeasureEntry current;
    ushort rowCount;
    ushort measure;
    private bool atEnd;
    
    public bool Parse()
    {
        while (!atEnd)
        {
            if (!ConsumeRow())
                return false;
        }
        
        return true;
    }

    private void SkipWhiteSpaces()
    {
        while (!src.IsEmpty && char.IsWhiteSpace(src[0])) src = src[1..];
        atEnd = src.IsEmpty;
    }

    private void SkipUntil(char c)
    {
        while (!src.IsEmpty)
        {
            if (src[0] == c) break;
            src = src[1..];
        }
        
        atEnd = src.IsEmpty;
    }

    private bool ConsumeNote(byte column)
    {
        SkipWhiteSpaces();
        if (atEnd) return false;

        switch (char.ToUpper(src[0]))
        {
            case '0': current.row[column] = SSCNoteType.Empty; break;
            case '1': current.row[column] = SSCNoteType.Tap; break;
            case '2': current.row[column] = SSCNoteType.HoldHead; break;
            case '3': current.row[column] = SSCNoteType.Tail; break;
            case '4': current.row[column] = SSCNoteType.RollHead; break;
            case 'M': current.row[column] = SSCNoteType.Mine; break;
            case 'L': current.row[column] = SSCNoteType.Lift; break;
            case 'F': current.row[column] = SSCNoteType.Fake; break;
            // ignored
            case 'K': break;
            default: return false;
        }
        
        src = src[1..];
        atEnd = src.IsEmpty;
        
        SkipWhiteSpaces();
        if (!atEnd && src[0] == '[') SkipUntil(']');
        SkipWhiteSpaces();
        if (!atEnd && src[0] == '{') SkipUntil('}');
        
        return true;
    }

    private bool ConsumeRow()
    {
        for (byte i = 0; i < SSCNoteRow.Length; ++i) if(!ConsumeNote(i)) return false;
        rowCount++;
        entries.Add(current);
        current.numerator = rowCount;
        current.measure = measure;
        
        SkipWhiteSpaces();
        if (!atEnd)
        {
            if (src[0] == ';')
            {
                atEnd = true;
            }
            // new measure
            else if (src[0] == ',')
            {
                src = src[1..];
                atEnd = src.IsEmpty;
                
                for (var i = entries.Count - rowCount; i < entries.Count; ++i)
                    entries[i] = entries[i] with {numerator = (ushort)(entries[i].measure * 4 * rowCount + entries[i].numerator * 4) , denominator = rowCount};
                rowCount = 0;
                measure++;
                current.numerator = rowCount;
			}
        }
        
        return true;
    }

    // requires the note data to be formatted
    public static void ParsePretty(ReadOnlySpan<char> src, List<SSCMeasureEntry> entries)
    {
        ushort measure = 0;
        foreach (var measureSplit in src.Split(','))
        {
            var measureSrc = src[measureSplit].Trim();

            var subdivision = measureSrc.Count('\n') + 1;
            ushort row = 0;
            foreach (var rowSplit in measureSrc.Split('\n'))
            {
                var lineSrc = measureSrc[rowSplit].Trim();

                var beat = new Beat((ushort)(measure * 4 * subdivision + row * 4), (ushort)subdivision);

                var entry = new SSCMeasureEntry { beat = beat };

                for (byte i = 0; i < lineSrc.Length; ++i)
                {
                    if (lineSrc[i] == '[')
                    {
                        while(i < lineSrc.Length)
                        {
                            if (lineSrc[i] == ']') break;
                            i++;
                        }
                    }
					if (lineSrc[i] == '{')
					{
						while (i < lineSrc.Length)
						{
							if (lineSrc[i] == '}') break;
							i++;
						}
					}

					entry.row[i] = lineSrc[i] switch
                    {
                        '0' => SSCNoteType.Empty,
                        '1' => SSCNoteType.Tap,
                        '2' => SSCNoteType.HoldHead,
                        '3' => SSCNoteType.Tail,
                        '4' => SSCNoteType.RollHead,
                        'M' => SSCNoteType.Mine,
                        'L' => SSCNoteType.Lift,
                        'F' => SSCNoteType.Fake,
                        _ => SSCNoteType.Empty, // ignore invalid values
                    };
                }

                entries.Add(entry);

                row++;
            }

            measure++;
        }
    }
}

