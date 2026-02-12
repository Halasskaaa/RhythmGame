#define FLIP_SCROLL_DIRECTION

using SDL3;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using wah.Chart.SSC;

namespace wah.Scenes;

internal class PlayerScene(SSCSimfile simfile, uint chartIndex) : IScene
{
    private bool         started;
    private Audio.Audio  audio      = new(simfile.Music!);
    private TimeSpan     startDelay = TimeSpan.FromSeconds(3);
    private ColumnStates columnStates;
    private Note[]       notes = CreateNotesFrom(simfile.Charts[chartIndex].Notes);

    // Countdown
    private int      countdownNumber = 3;
    private TimeSpan countdownTimer  = TimeSpan.FromSeconds(1);
    private bool     countdownOver => countdownNumber <= 0;

    // Judgments
    private const float perfectWindow   = 0.5f;
    private const float goodWindow      = 0.8f;
    private const float badIgnoreWindow = 0.9f;
    public const  float holdReHoldTime  = 1.0f;

    private static readonly SDL.FColor defaultGlowColor     = new(0.35f, 0.35f, 0.6f, 1f);
    private static readonly SDL.FColor perfectGlowColor     = new(0f, 0f, 1f, 1f);
    private static readonly SDL.FColor goodGlowColor        = new(0f, 1f, 0f, 1f);
    private static readonly SDL.FColor badGlowColor         = new(1f, 0f, 0f, 1f);
    private static readonly SDL.FColor bgColor              = new(0.08f, 0.08f, 0.1f, 1f);
    private static readonly SDL.FColor columnColor          = new(0.15f, 0.15f, 0.2f, 1f);
    private static readonly SDL.FColor hitLineColor         = new(1f, 1f, 1f, 1f);
    private static readonly SDL.FColor columnSeparatorColor = new(0.05f, 0.05f, 0.05f, 1f);
    private static readonly SDL.FColor noteColor            = new(1f, 1f, 0f, 1f);
    private static readonly SDL.FColor mineColor            = new(1f, 0f, 0f, 1f);
    private static readonly SDL.FColor liftColor            = new(0f, 1f, 0f, 1f);
    private static readonly SDL.FColor holdBodyColor        = new(0.4f, 0.4f, 0f, 1f);

    // Combo & Score
    private int combo = 0;
    private int score = 0;

    public void OnInput(in InputEvent input)
    {
        if (!countdownOver) return;
        if (input.Type != InputEvent.EventType.Key) return;

        var k = input.Key;
        if (k.Repeat) return;

		if (k.Key == SDL.Keycode.Escape) SceneManager.Current = new TitleScreen();

		ref var state = ref Unsafe.NullRef<ColumnInputState>();

        switch (k.Key)
        {
            case SDL.Keycode.D: state = ref columnStates[0]; break;
            case SDL.Keycode.F: state = ref columnStates[1]; break;
            case SDL.Keycode.J: state = ref columnStates[2]; break;
            case SDL.Keycode.K: state = ref columnStates[3]; break;
        }

        if (Unsafe.IsNullRef(ref state)) return;

        state.pressing         = k.Down;
        state.pressedThisFrame = state.pressing;
        state.relasedThisFrame = !state.pressing;
        state.glowValue        = 1;
        state.glowColor        = defaultGlowColor;
    }

    public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
    {
        const float columnWidth        = 200f;
        const float noteHeight         = 10f;
        const float columnTopOffset    = 100;
        const float columnBottomOffset = 2 * columnTopOffset;
        var         area               = renderer.RenderArea;
        var         hitLineY           = columnTopOffset + (area.H - columnBottomOffset);

        // Countdown
        if (!started)
        {
            startDelay     -= deltaTime;
            countdownTimer -= deltaTime;

            if (countdownTimer.Ticks <= 0 && countdownNumber > 0)
            {
                countdownNumber--;
                countdownTimer = TimeSpan.FromSeconds(1);
            }

            if (countdownNumber <= 0 && startDelay.Ticks <= 0)
            {
                started = true;
                audio.Play();
            }
        }

        var currentBeat = BeatAt((float)audio.PlayBackPosition.TotalSeconds + simfile.Offset - (float)startDelay.TotalSeconds);

        // -------------------------
        // Background
        renderer.DrawRectFilled(
                                new SDL.FRect { X = 0, Y = 0, W = area.W, H = area.H },
                                bgColor
                               );

        // -------------------------
        // Columns + glow
        for (byte i = 0; i < 4; i++)
        {
            ref var    state = ref columnStates[i];
            SDL.FColor col   = LerpColor(columnColor, state.glowColor, state.glowValue);

            renderer.DrawRectFilled(
                                    new SDL.FRect
                                    {
                                        X = columnWidth * i, Y = columnTopOffset, W = columnWidth,
                                        H = area.H - columnBottomOffset
                                    },
                                    col
                                   );

            renderer.DrawRectFilled(
                                    new SDL.FRect
                                    {
                                        X = columnWidth * i + columnWidth - 2, Y = columnTopOffset, W = 2,
                                        H = area.H                        - columnBottomOffset
                                    },
                                    columnSeparatorColor
                                   );

            state.glowValue = Math.Max(0f, state.glowValue - (float)deltaTime.TotalSeconds * 2f);
        }

        // -------------------------
        // Notes + judgement
        const float distanceBetweenBeats = 400f;
        float noteSpeed = distanceBetweenBeats * GameSettings.noteSpeed;
        foreach (ref var note in notes.AsSpan())
        {
            // note.singleNote.judgeTime and note.holdNote.judgeTime are in the same place
            // and so is judgeState.judgedTime
                
            // unjudged notes have a nan value for this
            if (!float.IsNaN(note.singleNote.judgeState.judgedTime) &&
                note.type is not (NoteType.Hold or NoteType.Roll)) continue;
            // negativeInf=failed & positiveInf=passed
            if (note.type is (NoteType.Hold or NoteType.Roll) &&
                float.IsInfinity(note.holdNote.holdJudgeState.lastHeldAt)) continue;

            float noteY =
#if FLIP_SCROLL_DIRECTION
                area.H + (currentBeat - note.singleNote.judgeTime)
#else
                    (note.singleNote.judgeTime - currentBeat)
#endif
                * noteSpeed + columnTopOffset;

#if FLIP_SCROLL_DIRECTION
            if (noteY < columnTopOffset - noteHeight) continue;
#else
            if (noteY > area.H - columnBottomOffset - noteHeight) continue;
#endif

            // Draw note
            // hold/roll body first
            if (note.type is not (NoteType.Hold or NoteType.Roll)) goto drawSingle;
            // hold body
            renderer.DrawRectFilled(
                                    new SDL.FRect
                                    {
                                        X = columnWidth * note.column,
                                        Y = noteY,
                                        W = columnWidth,
                                        H = (note.holdNote.endTime - note.holdNote.judgeTime) * noteSpeed
#if FLIP_SCROLL_DIRECTION
                                                                                              * -1f
#endif
                                    },
                                    LerpColor(holdBodyColor, columnColor,
                                              float.IsNaN(note.holdNote.holdJudgeState.judgedTime)
                                                  ? 0
                                                  : InverseLerp(note.holdNote.holdJudgeState.lastHeldAt,
                                                                note.holdNote.holdJudgeState.lastHeldAt +
                                                                holdReHoldTime, currentBeat))
                                   );

            drawSingle:
            renderer.DrawRectFilled(
                                    new SDL.FRect
                                    { X = columnWidth * note.column, Y = noteY, W = columnWidth, H = noteHeight },
                                    note.type switch
                                    {
                                        NoteType.Mine => mineColor,
                                        NoteType.Lift => liftColor,
                                        _             => noteColor
                                    }
                                   );

            // judgement
            ref var column = ref columnStates[note.column];
            if (note.type is NoteType.Tap or NoteType.Hold or NoteType.Roll &&
                float.IsNaN(note.singleNote.judgeState.judgedTime))
            {
                var diff  = note.singleNote.judgeTime - currentBeat;
                var aDiff = MathF.Abs(diff);

                // miss
                if (diff < -goodWindow)
                {
                    note.singleNote.judgeState.judgedTime = currentBeat;
                    combo                                 = 0;
                    if (note.type is NoteType.Hold or NoteType.Roll)
                    {
                        note.holdNote.holdJudgeState.lastHeldAt = float.NegativeInfinity;
                    }
                }

                if (column.pressedThisFrame)
                {
                    // perfect or good
                    if (aDiff < goodWindow)
                    {
                        note.singleNote.judgeState.judgedTime = currentBeat;
                        combo++;
                        score++;
                        column.glowColor        = aDiff < perfectWindow ? perfectGlowColor : goodGlowColor;
                        column.pressedThisFrame = false;
                    }

                    // bad
                    else if (diff < badIgnoreWindow)
                    {
                        note.singleNote.judgeState.judgedTime = currentBeat;
                        combo                                 = 0;
                        column.glowColor                      = badGlowColor;
                        column.pressedThisFrame               = false;

                        if (note.type is NoteType.Hold or NoteType.Roll)
                        {
                            note.holdNote.holdJudgeState.lastHeldAt = float.NegativeInfinity;
                        }
                    }
                }
            }
            else if (note.type is NoteType.Hold)
            {
                if (column.pressing)
                {
                    note.holdNote.holdJudgeState.lastHeldAt = currentBeat;
                }

                if (currentBeat - note.holdNote.holdJudgeState.lastHeldAt > holdReHoldTime)
                {
                    note.holdNote.holdJudgeState.lastHeldAt = float.NegativeInfinity;
                    combo                                   = 0;
                }

                if (currentBeat >= note.holdNote.endTime)
                {
                    note.holdNote.holdJudgeState.lastHeldAt = float.PositiveInfinity;
                    score++;
                    combo++;
                }
            }
            else if (note.type is NoteType.Roll)
            {
                // just this much should make it work... hopefully
                if (column.pressedThisFrame)
                {
                    note.holdNote.holdJudgeState.lastHeldAt = currentBeat;
                    column.pressedThisFrame = false;
                }

                if (currentBeat - note.holdNote.holdJudgeState.lastHeldAt > holdReHoldTime)
                {
                    note.holdNote.holdJudgeState.lastHeldAt = float.NegativeInfinity;
                    combo                                   = 0;
                }

                if (currentBeat >= note.holdNote.endTime)
                {
                    note.holdNote.holdJudgeState.lastHeldAt = float.PositiveInfinity;
                    score++;
                    combo++;
                }
            }
            else if (note.type is NoteType.Mine)
            {
                if (column.pressing && MathF.Abs(note.singleNote.judgeTime - currentBeat) < goodWindow)
                {
                    note.singleNote.judgeState.judgedTime = currentBeat;
                    combo                                 = 0;
                    score--;
                }
            }
            else if (note.type is NoteType.Lift)
            {
                var diff  = note.singleNote.judgeTime - currentBeat;
                var aDiff = MathF.Abs(diff);

                // miss
                if (diff < -goodWindow)
                {
                    note.singleNote.judgeState.judgedTime = currentBeat;
                    combo                                 = 0;
                }

                if (column.relasedThisFrame)
                {
                    // perfect or good
                    if (aDiff < goodWindow)
                    {
                        note.singleNote.judgeState.judgedTime = currentBeat;
                        combo++;
                        score++;
                        column.glowColor        = aDiff < perfectWindow ? perfectGlowColor : goodGlowColor;
                        column.relasedThisFrame = false;
                    }

                    // bad
                    else if (diff < badIgnoreWindow)
                    {
                        note.singleNote.judgeState.judgedTime = currentBeat;
                        combo                                 = 0;
                        column.glowColor                      = badGlowColor;
                        column.relasedThisFrame               = false;
                    }
                }
            }
        }

        // -------------------------
        // Hit line
        renderer.DrawRectFilled(
                                new SDL.FRect { X = 0, Y = hitLineY, W = columnWidth * 4, H = 4 },
                                hitLineColor
                               );

        // -------------------------
        // Key caps
        for (byte i = 0; i < SSCNoteRow.Length; ++i)
        {
            // hide hold bodies & missed notes
            renderer.DrawRectFilled(
                                    new SDL.FRect { X = columnWidth * i, Y = hitLineY, W = columnWidth, H = area.H },
                                    columnColor
                                   );

            renderer.DrawRectFilled(
                                    new SDL.FRect
                                    { X = columnWidth * i + 20, Y = hitLineY + 20, W = columnWidth - 40, H = 60 },
                                    new SDL.FColor(0.25f, 0.25f, 0.3f, 1f)
                                   );

            renderer.DrawText(
                              i switch
                              {
                                  0 => "D",
                                  1 => "F",
                                  2 => "J",
                                  3 => "K",
                                  _ => throw new UnreachableException()
                              },
                              columnWidth * i + columnWidth / 2 - 6,
                              hitLineY                          + 35,
                              new SDL.FColor(1f, 1f, 1f, 1f)
                             );
        }

        var judgmentYFixed = hitLineY - 50;

        // Draw combo & score
        if (combo > 1)
            renderer.DrawText($"COMBO {combo}", area.W / 2 - 50, judgmentYFixed - 40, new SDL.FColor(1f, 0.8f, 0f, 1f));
        renderer.DrawText($"SCORE {score}", area.W / 2 - 50, judgmentYFixed - 70, new SDL.FColor(1f, 1f, 1f, 1f));

        for (byte i = 0; i < ColumnStates.Length; ++i)
        {
            columnStates[i].relasedThisFrame = false;
            columnStates[i].pressedThisFrame = false;
        }

        // countdown
        if (!started)
        {
            float x = columnWidth * 2 - 20;
            float y = area.H      / 2 - 20;
            renderer.DrawText(countdownNumber.ToString(), x, y, new SDL.FColor(1f, 1f, 1f, 1f));
        }

        if (audio.PlayBackPosition == audio.Length) SceneManager.Current = new SongSelectScreen();
    }

    private float BeatAt(float seconds)
    {
        float secondsPassed = 0f;
        float beatsPassed   = 0f;

        for (var i = 0; i < simfile.BPMs.Length; i++)
        {
            var     rem = seconds - secondsPassed;
            ref var bpm = ref simfile.BPMs[i];

            if (i == simfile.BPMs.Length - 1)
                return beatsPassed       + rem / 60 * bpm.BPM;

            ref var next        = ref simfile.BPMs[i + 1];
            float   durationSec = (next.FromBeat - bpm.FromBeat) / bpm.BPM * 60f;

            if (rem < durationSec)
                return beatsPassed + rem / 60 * bpm.BPM;

            secondsPassed += durationSec;
            beatsPassed   += durationSec / 60 * bpm.BPM;
        }

        throw new UnreachableException();
    }

    private static SDL.FColor LerpColor(SDL.FColor a, SDL.FColor b, float t)
    {
        return new SDL.FColor(
                              a.R + (b.R - a.R) * t,
                              a.G + (b.G - a.G) * t,
                              a.B + (b.B - a.B) * t,
                              1f
                             );
    }

    private static float InverseLerp(float from, float to, float value)
    {
        // v = f + (f-t) * p
        return (value - from) / (to - from);
    }

    [InlineArray(Length)]
    private struct ColumnStates
    {
        public const byte             Length = SSCNoteRow.Length;
        private      ColumnInputState _;
    }

    private struct ColumnInputState
    {
        public SDL.FColor glowColor;
        public float      glowValue;
        public bool       pressing, pressedThisFrame, relasedThisFrame;
    }

    // size: 4, align: 4
    private struct SingleNoteJudgeState
    {
        public float judgedTime;
    }

    // size: 8, align: 4
    private struct HoldNoteJudgeState
    {
        public float judgedTime;
        public float lastHeldAt;
    }

    // size: 12, align: 4
    private struct RollJudgeState
    {
        public float judgedTime;
        public float releasedSince;
        public float heldSince;
    }

    // size: 8, align: 4
    private struct SingleNote
    {
        public float                judgeTime;
        public SingleNoteJudgeState judgeState;
    }

    // size: 20, align: 4
    [StructLayout(LayoutKind.Explicit)]
    private struct HoldNote
    {
        [FieldOffset(0 * sizeof(float))] public float              judgeTime;
        [FieldOffset(1 * sizeof(float))] public RollJudgeState     rollJudgeState;
        [FieldOffset(1 * sizeof(float))] public HoldNoteJudgeState holdJudgeState;
        [FieldOffset(4 * sizeof(float))] public float              endTime;
    }

    // size: 1, align: 1
    private enum NoteType : byte
    {
        Tap,
        Mine,
        Lift,
        Hold,
        Roll
    }

    // size: 24 (23+1), align: 4
    [StructLayout(LayoutKind.Explicit)]
    private struct Note
    {
        [FieldOffset(0 * sizeof(float) + 0 * sizeof(byte))]
        public SingleNote singleNote;

        [FieldOffset(0 * sizeof(float) + 0 * sizeof(byte))]
        public HoldNote holdNote;

        [FieldOffset(5 * sizeof(float) + 0 * sizeof(byte))]
        public NoteType type;

        [FieldOffset(5 * sizeof(float) + 1 * sizeof(byte))]
        public byte column;

        [FieldOffset(5 * sizeof(float) + 2 * sizeof(byte))]
        public bool isFake;
    }

    // // size: 24 (21+3), align: 4
    // [StructLayout(LayoutKind.Explicit)]
    // private struct Note
    // {
    //     [FieldOffset(0 * sizeof(float))] public SingleNote singleNote;
    //     [FieldOffset(0 * sizeof(float))] public HoldNote   holdNote;
    //     [FieldOffset(5 * sizeof(float))] public byte   ctrl;
    //     // ctrl:
    //     // 8 bits
    //     // 0b00000001 (1st)    : isFake
    //     // 0b00000110 (2nd-3rd): column
    //     // 0b00000000 (4th-7th): type
    // }

    private static Note[] CreateNotesFrom(ReadOnlySpan<SSCMeasureEntry> sscNotes)
    {
        var notes     = new Note[sscNotes.Length * SSCNoteRow.Length];
        var noteCount = 0;

        for (ushort i = 0; i < sscNotes.Length; ++i)
        {
            var beat = sscNotes[i].beat.Value;
            for (byte j = 0; j < SSCNoteRow.Length; ++j)
            {
                var type = sscNotes[i].row[j];
                if (type is SSCNoteType.Empty or SSCNoteType.Tail) continue;
                ref var note = ref notes[noteCount++];

                switch (type)
                {
                    case SSCNoteType.Tap:
                        note.type = NoteType.Tap;
                        goto case SSCNoteType.Fake;
                    case SSCNoteType.Mine:
                        note.type = NoteType.Mine;
                        goto case SSCNoteType.Fake;
                    case SSCNoteType.Lift:
                        note.type = NoteType.Lift;
                        goto case SSCNoteType.Fake;
                    case SSCNoteType.Fake:
                        note.singleNote.judgeTime             = beat;
                        note.singleNote.judgeState.judgedTime = float.NaN;
                        note.column                           = j;
                        note.isFake                           = type is SSCNoteType.Fake;
                        break;
                    case SSCNoteType.HoldHead:
                    case SSCNoteType.RollHead:
                        note.type = type is SSCNoteType.HoldHead ? NoteType.Hold : NoteType.Roll;
                        note.holdNote.judgeTime = beat;
                        note.holdNote.holdJudgeState.judgedTime = float.NaN; // judgedTime is at the same place for both
                        note.column = j;
                        for (var k = i; k < sscNotes.Length; ++k)
                        {
                            if (sscNotes[k].row[j] is not SSCNoteType.Tail) continue;
                            note.holdNote.endTime = sscNotes[k].beat.Value;
                            break;
                        }

                        break;
                    case SSCNoteType.Tail:
                    case SSCNoteType.Empty:
                    default:
                        throw new UnreachableException();
                }
            }
        }

        Array.Resize(ref notes, noteCount);

        return notes;
    }
}
