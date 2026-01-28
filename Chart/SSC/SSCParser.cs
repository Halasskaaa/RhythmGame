using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using wah.Util;

namespace wah.Chart.SSC
{
    // edge cases:
    // - StepMania tries four encodings (UTF-8 followed by three code pages) in order until one succeeds.
    // -  formatting errors such as malformed comments and missing semicolons.
    //    StepMania handles missing semicolons at the protocol level and emits a warning for other formatting errors.
    // - Holds and rolls are expected to have corresponding tail notes; a head note without a tail note (or vice-versa) is an error.
    //   StepMania emits a warning and treats disconnected head notes as tap notes (and discards orphaned tail notes).
    // - Some properties have legacy aliases, like FREEZES in place of STOPS.
    //   Additionally, keysounded SSC charts use a NOTES2 property for note data instead of the usual NOTES.
    //   StepMania looks for these aliases in the absence of the regular property name.
    // - During development of the SSC format, timing data on charts (“split timing”) was an unstable experimental feature.
    //   Modern versions of StepMania ignore timing data from these unstable versions (prior to version 0.70).
    internal ref struct SSCParser(ReadOnlySpan<char> src, DirectoryInfo chartRoot)
    {
        ReadOnlySpan<char> src = src;

        ushort cursor;
        //private bool ParseChart(out SSCChart chart, Action<string>? emitWarning = null)
        //{
        //	chart = default;

        //	emitWarning ??= Console.WriteLine;


        //	return true;
        //}

        public bool Parse(out SSCSimfile simfile)
        {
            simfile = default;

            bool versionParsed          = false;
            bool titleParsed            = false;
            bool subtitleParsed         = false;
            bool artistParsed           = false;
            bool titleTranslitParsed    = false;
            bool subtitleTranslitParsed = false;
            bool artistTranslitParsed   = false;
            bool genreParsed            = false;
            bool creditParsed           = false;
            bool bannerParsed           = false;
            bool backgroundParsed       = false;
            bool lyricsPathParsed       = false;
            bool cdTitleParsed          = false;
            bool musicParsed            = false;
            bool instrumentTrackParsed  = false;
            bool musicLengthParsed      = false;
            bool offsetParsed           = false;
            bool bpmsParsed             = false;
            bool stopsParsed            = false;
            bool delaysParsed           = false;
            bool labelsParsed           = false;
            bool timeSignaturesParsed   = false;
            bool sampleStartParsed      = false;
            bool sampleLengthParsed     = false;
            bool displayBPMParsed       = false;
            bool selectableParsed       = false;
            bool lastSecondHintParsed   = false;
            bool bgChangesParsed        = false;
            bool fgChangesParsed        = false;
            bool keySoundsParsed        = false;
            bool attacksParsed          = false;

            List<SSCChart> charts = [];

            while (cursor != src.Length)
            {
                ConsumeWhiteSpacesAndComments();
                if (cursor >= src.Length) break;
                if (!ParseProperty(out var name, out var value, true)) return false;

                Debug.WriteLine($"parsing \"{name}\" with a value of \"{value}\"");
                if (value.IsEmpty && !name.SequenceEqual("NOTEDATA")) continue;

                switch (name)
                {
                    case "VERSION":
                        if (!versionParsed)
                        {
                            versionParsed = true;
                            if (!float.TryParse(value, CultureInfo.InvariantCulture, out var version)) return false;
                            simfile = simfile with { Version = new(version) };
                        }

                        break;
                    case "TITLE":
                        if (!titleParsed)
                        {
                            titleParsed = true;
                            simfile     = simfile with { Title = simfile.Title with { Value = value.ToString() } };
                        }

                        break;
                    case "SUBTITLE":
                        if (!subtitleParsed)
                        {
                            subtitleParsed = true;
                            simfile = simfile with { Subtitle = simfile.Subtitle with { Value = value.ToString() } };
                        }

                        break;
                    case "ARTIST":
                        if (!artistParsed)
                        {
                            artistParsed = true;
                            simfile      = simfile with { Artist = simfile.Artist with { Value = value.ToString() } };
                        }

                        break;
                    case "TITLETRANSLIT":
                        if (!titleTranslitParsed)
                        {
                            titleTranslitParsed = true;
                            simfile = simfile with { Title = simfile.Title with { Translit = value.ToString() } };
                        }

                        break;
                    case "SUBTITLETRANSLIT":
                        if (!subtitleTranslitParsed)
                        {
                            subtitleTranslitParsed = true;
                            simfile = simfile with { Subtitle = simfile.Subtitle with { Translit = value.ToString() } };
                        }

                        break;
                    case "ARTISTTRANSLIT":
                        if (!artistTranslitParsed)
                        {
                            artistTranslitParsed = true;
                            simfile = simfile with { Artist = simfile.Artist with { Translit = value.ToString() } };
                        }

                        break;
                    case "GENRE":
                        if (!genreParsed)
                        {
                            genreParsed = true;
                            simfile     = simfile with { Genre = value.ToString() };
                        }

                        break;
                    case "CREDIT":
                        if (!creditParsed)
                        {
                            creditParsed = true;
                            simfile      = simfile with { Credit = value.ToString() };
                        }

                        break;
                    case "BANNER":
                        if (!bannerParsed)
                        {
                            bannerParsed = true;
                            simfile      = simfile with { Banner = chartRoot.AppendPath(value) };
                        }

                        break;
                    case "BACKGROUND":
                        if (!backgroundParsed)
                        {
                            backgroundParsed = true;
                            simfile          = simfile with { Background = chartRoot.AppendPath(value) };
                        }

                        break;
                    case "LYRICSPATH":
                        if (!lyricsPathParsed)
                        {
                            lyricsPathParsed = true;
                            simfile          = simfile with { LyricsPath = chartRoot.AppendPath(value) };
                        }

                        break;
                    case "CDTITLE":
                        if (!cdTitleParsed)
                        {
                            cdTitleParsed = true;
                            simfile       = simfile with { CDTitle = chartRoot.AppendPath(value) };
                        }

                        break;
                    case "MUSIC":
                        if (!musicParsed)
                        {
                            musicParsed = true;
                            simfile     = simfile with { Music = chartRoot.AppendPath(value) };
                        }

                        break;
                    case "INSTRUMENTTRACK":
                        if (!instrumentTrackParsed)
                        {
                            instrumentTrackParsed = true;
                            if (!InstrumentTrack.TryParse(value, chartRoot, out var instrumentTrack)) return false;
                            simfile = simfile with { InstrumentTrack = instrumentTrack };
                        }

                        break;
                    case "MUSICLENGTH": // cache
                        if (!musicLengthParsed)
                        {
                            musicLengthParsed = true;
                            if (!float.TryParse(value, out var musicLength)) return false;
                            simfile = simfile with { MusicLength = musicLength };
                        }

                        break;
                    case "OFFSET":
                        if (!offsetParsed)
                        {
                            offsetParsed = true;
                            if (!float.TryParse(value, out var offset)) return false;
                            simfile = simfile with { Offset = offset };
                        }

                        break;
                    case "BPMS":
                        if (!bpmsParsed)
                        {
                            bpmsParsed = true;
                            List<BPMChange> changes = [];
                            foreach (var range in value.Split(','))
                            {
                                if (!BPMChange.TryParse(value[range], out var bpmChange)) return false;
                                changes.Add(bpmChange);
                            }

                            simfile = simfile with { BPMs = [.. changes] };
                        }

                        break;
                    case "FREEZES": // alias
                    case "STOPS":
                        if (!stopsParsed)
                        {
                            stopsParsed = true;
                        }

                        break;
                    case "DELAYS":
                        if (!delaysParsed)
                        {
                            delaysParsed = true;

                            List<Delay> delays = [];
                            foreach (var range in value.Split(','))
                            {
                                if (!Delay.TryParse(value[range], out var delay)) return false;
                                delays.Add(delay);
                            }

                            simfile = simfile with { Delays = [.. delays] };
                        }

                        break;
                    case "LABELS":
                        if (!labelsParsed)
                        {
                            labelsParsed = true;
                        }

                        break;
                    case "TIMESIGNATURES":
                        if (!timeSignaturesParsed)
                        {
                            timeSignaturesParsed = true;
                            List<TimeSignatureChange> changes = [];
                            foreach (var range in value.Split(','))
                            {
                                if (!TimeSignatureChange.TryParse(value[range], out var change)) return false;
                                changes.Add(change);
                            }

                            simfile = simfile with { TimeSignatures = [.. changes] };
                        }

                        break;
                    case "SAMPLESTART":
                        if (!sampleStartParsed)
                        {
                            sampleStartParsed = true;
                            if (!float.TryParse(value, out var sampleStart)) return false;
                            simfile = simfile with { SampleStart = sampleStart };
                        }

                        break;
                    case "SAMPLELENGTH":
                        if (!sampleLengthParsed)
                        {
                            sampleLengthParsed = true;
                            if (!float.TryParse(value, out var sampleLength)) return false;
                            simfile = simfile with { SampleLength = sampleLength };
                        }

                        break;
                    case "DISPLAYBPM":
                        if (!displayBPMParsed)
                        {
                            displayBPMParsed = true;
                            simfile          = simfile with { DisplayBPM = value.ToString() };
                        }

                        break;
                    case "SELECTABLE":
                        if (!selectableParsed)
                        {
                            selectableParsed = true;
                            if (!TryParseYesNo(value, out var selectable)) return false;
                            simfile = simfile with { Selectable = selectable };
                        }

                        break;
                    case "LASTSECONDHINT":
                        if (!lastSecondHintParsed)
                        {
                            lastSecondHintParsed = true;
                        }

                        break;
                    case "ANIMATIONS": // alias
                    case "BGCHANGES2": // alias
                    case "BGCHANGES3": // alias
                    case "BGCHANGES":
                        if (!bgChangesParsed)
                        {
                            bgChangesParsed = true;
                            List<BGChange> bgChanges = [];
                            foreach (var range in value.Split(','))
                            {
                                var entry = value[range].Trim();
                                if (!BGChange.TryParse(entry, chartRoot, out var bgChange)) return false;
                                bgChanges.Add(bgChange with
                                              {
                                                  OnLayer = name[-1] switch
                                                            {
                                                                '2' => BGChange.Layer.BG2,
                                                                '3' => BGChange.Layer.BG3,
                                                                _   => BGChange.Layer.BG1,
                                                            }
                                              });
                            }

                            simfile = simfile with { BGChanges = [.. bgChanges] };
                        }

                        break;
                    case "FGCHANGES":
                        if (!fgChangesParsed)
                        {
                            fgChangesParsed = true;
                            List<BGChange> bgChanges = [];
                            foreach (var range in value.Split(','))
                            {
                                var entry = value[range].Trim();
                                if (!BGChange.TryParse(entry, chartRoot, out var bgChange)) return false;
                                bgChanges.Add(bgChange with
                                              {
                                                  OnLayer = BGChange.Layer.FG1
                                              });
                            }

                            simfile = simfile with { FGChanges = [.. bgChanges] };
                        }

                        break;
                    case "KEYSOUNDS":
                        if (!keySoundsParsed)
                        {
                            keySoundsParsed = true;
                            List<FileInfo> files = [];
                            foreach (var file in value.Split(","))
                                files.Add(new FileInfo(Path.Combine(chartRoot.FullName, file.ToString())));
                            simfile = simfile with { KeySounds = [.. files] };
                        }

                        break;
                    case "ATTACKS":
                        if (!attacksParsed)
                        {
                            attacksParsed = true;

                            /*
                                #ATTACKS:TIME=1.618:END=3.166:MODS=*32 Invert, *32 No Flip
                                :TIME=2.004:END=3.166:MODS=*32 No Invert, *32 No Flip
                                :TIME=2.392:LEN=0.1:MODS=*64 30% Mini
                                :TIME=2.489:LEN=0.1:MODS=*64 60% Mini;
                             */

                            // state
                            bool timeParsed = false, endParsed = false, modParsed = false, endIsLen = false;
                            float time = 0, end = 0;
                            ReadOnlySpan<char> mod = [];

                            List<Attack> attacks = [];

                            foreach (var range in value.Split(':'))
                            {
                                var kv       = value[range].Trim();
                                var splitIdx = kv.IndexOf('=');
                                if (splitIdx < 0) return false;

                                var key   = kv[..(splitIdx + 1)];
                                var param = kv[(splitIdx   + 1)..]; // "value" name is already used

                                switch (key)
                                {
                                    case "TIME":
                                    {
                                        if (timeParsed) return false;
                                        timeParsed = true;
                                        if (!float.TryParse(param, out time)) return false;
                                        break;
                                    }
                                    case "END":
                                    {
                                        if (endParsed) return false;
                                        endParsed = true;
                                        if (float.TryParse(param, out end)) return false;
                                        break;
                                    }
                                    case "LEN":
                                    {
                                        if (endParsed) return false;
                                        endParsed = true;
                                        if (float.TryParse(param, out end)) return false;
                                        endIsLen = true;
                                        break;
                                    }
                                    case "MOD":
                                    {
                                        if (modParsed) return false;
                                        modParsed = true;
                                        mod       = param;
                                        break;
                                    }
                                }

                                if (timeParsed && endParsed && modParsed)
                                {
                                    attacks.Add(new Attack(time, endIsLen ? time + end : end, mod.ToString()));
                                    timeParsed = false;
                                    endParsed  = false;
                                    modParsed  = false;
                                    endIsLen   = false;
                                }
                            }

                            simfile = simfile with { Attacks = [.. attacks] };
                        }

                        break;
                    case "NOTEDATA": // new notes section
                        if (!ParseSteps(out var chart)) return false;
                        charts.Add(chart);
                        break;
                }
            }

            if (!musicLengthParsed) // TODO
                simfile = simfile with { MusicLength = -1 };
            simfile = simfile with { Charts = [.. charts] };
            return true;
        }

        bool ParseSteps(out SSCChart chart)
        {
            chart = default;

            bool chartNameParsed   = false;
            bool stepStypeParsed   = false;
            bool descriptionParsed = false;
            bool difficultyParsed  = false;
            bool meterParsed       = false;
            bool radarValuesParsed = false;
            bool creditParsed      = false;

            while (cursor < src.Length)
            {
                ConsumeWhiteSpacesAndComments();
                if (cursor >= src.Length) break;

                if (ConsumeIfMatch("#NOTES:") || ConsumeIfMatch("#NOTES2:"))
                {
                    // assume that notes is the last entry
                    var cursorNow = cursor;
                    if (!ConsumeUntil(";", false)) return false;
                    var noteData = src[cursorNow..cursor++].Trim();
                    var notes = new List<SSCMeasureEntry>(1024);
                    //if (!new SSCSteps(noteData, notes).Parse()) return false;
                    SSCSteps.ParsePretty(noteData, notes);

                    chart = chart with { Notes = [..notes] };

                    break;
                }
                else
                {
                    var cursorNow = cursor;
                    if (!ParseProperty(out var name, out var value, true)) return false;

                    Debug.WriteLine($"(chart) parsing \"{name}\" with a value of \"{value}\"");

                    switch (name)
                    {
                        case "CHARTNAME":
                            if (!chartNameParsed)
                            {
                                chartNameParsed = true;
                                chart           = chart with { ChartName = value.ToString() };
                            }

                            break;
                        case "STEPSTYPE":
                            if (!stepStypeParsed)
                            {
                                stepStypeParsed = true;

                                // only this mode is supported
                                if (!value.SequenceEqual("dance-single")) return false;
                                chart = chart with { StepsType = StepsType.StepsSingle };
                            }

                            break;
                        case "DESCRIPTION":
                            if (!descriptionParsed)
                            {
                                descriptionParsed = true;

                                chart = chart with { Description = value.ToString() };
                            }

                            break;
                        case "DIFFICULTY":
                            if (!difficultyParsed)
                            {
                                difficultyParsed = true;

                                chart = chart with { Difficulty = value.ToString() };
                                // TODO: if difficulty is smaniac or challenge, set difficulty to challenge
                            }

                            break;
                        case "METER":
                            if (!meterParsed)
                            {
                                meterParsed = true;

                                if (!float.TryParse(value, out var meter)) return false;

                                chart = chart with { Meter = meter };
                            }

                            break;
                        case "RADARVALUES":
                            if (!radarValuesParsed)
                            {
                                radarValuesParsed = true;

                                if (!RadarValues.TryParse(value, out var radarValues)) return false;
                                chart = chart with { RadarValues = radarValues };
                            }

                            break;
                        case "CREDIT":
                            if (!creditParsed)
                            {
                                creditParsed = true;
                                chart        = chart with { Credit = value.ToString() };
                            }

                            break;
                        default
                            : // when arriving at an unknown property, assume that we are back to parsing the sim file
                            // not that this should ever happen
                            cursor = cursorNow;
                            return true;
                    }
                }
            }

            // successful parsing != valid chart
            return true;
        }

        bool Consume(out char c)
        {
            c = default;
            if (cursor >= src.Length) return false;
            c = src[cursor++];
            return true;
        }

        bool Peek(out char c) => PeekN(out c, 0);

        bool PeekN(out char c, ushort n)
        {
            c = default;
            var offset = cursor + n;
            if (offset >= src.Length) return false;
            c = src[offset];
            return true;
        }

        bool Match(ReadOnlySpan<char> to)
        {
            for (ushort i = 0; i < to.Length; ++i)
                if (!PeekN(out var c, i) || c != to[i])
                    return false;
            return true;
        }

        bool ConsumeUntil(ReadOnlySpan<char> seq, bool consumeSeq)
        {
            while (!Match(seq))
                if (!Consume(out _))
                    return false;

            if (!consumeSeq) return true;

            for (var i = 0; i < seq.Length; ++i) Consume(out _);

            return true;
        }

        bool ConsumeWhitespaces()
        {
            while (true)
            {
                if (!Peek(out var c)) return false;
                if (char.IsWhiteSpace(c)) Consume(out _);
                else return true;
            }
        }

        bool ParseComment()
        {
            if (!Match("//")) return false;
            return ConsumeUntil("\n", true);
        }

        void ConsumeWhiteSpacesAndComments()
        {
            while (true)
                if (!ConsumeWhitespaces() || !ParseComment())
                    return;
        }

        bool ConsumeIfMatch(ReadOnlySpan<char> to)
        {
            if (Match(to))
            {
                for (var i = 0u; i < to.Length; ++i) Consume(out _);
                return true;
            }

            return false;
        }

        bool ParseProperty(out ReadOnlySpan<char> name, out ReadOnlySpan<char> value, bool forceEndOnNewLine)
        {
            name  = default;
            value = default;

            if (!Peek(out var c) || c != '#') return false;
            Consume(out _);

            var cursorNow = cursor;
            if (!ConsumeUntil(":", false)) return false;
            name = src[cursorNow..cursor++];

            if (!ConsumeWhitespaces()) return false;

            cursorNow = cursor;
            // TODO: clean this up
            while (true)
            {
                if (!Peek(out c)) return false;
                if (c == ';')
                {
                    value = src[cursorNow..cursor++];
                    return true;
                }

                if (forceEndOnNewLine)
                {
                    if (c == '\n')
                    {
                        value = src[cursorNow..cursor++];
                        return true;
                    }

                    if (c == '\r' && PeekN(out c, 1) && c == '\n')
                    {
                        value  =  src[cursorNow..cursor];
                        cursor += 2;
                        return true;
                    }

                    if (c      == '\r' &&
                        cursor == src.Length - 1) // edge case: invalid eol sequence at eof (\rEOF instead of \r\nEOF)
                    {
                        value = src[cursorNow..cursor++];
                        return true;
                    }
                }

                Consume(out _);
            }
        }

        static bool TryParseYesNo(ReadOnlySpan<char> src, out bool isYes)
        {
            isYes = default;
            Span<char> lower = stackalloc char[src.Length];
            src.ToLowerInvariant(lower);

            switch (lower)
            {
                case "yes":
                case "1":
                case "es":   // 3.9+
                case "omes": // 3.9+
                    isYes = true;
                    return true;
                case "no":
                case "0":
                    isYes = false;
                    return true;
                default:
                    return false;
            }
        }
    }
}
