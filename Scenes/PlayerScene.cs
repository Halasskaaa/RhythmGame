//#define FLIP_SCROLL_DIRECTION

using SDL3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using wah.Audio;
using wah.Chart.SSC;

namespace wah.Scenes;

internal class PlayerScene : IScene
{
    private bool started;
    private Audio.Audio audio = null!;
    private TimeSpan startDelay = TimeSpan.FromSeconds(3);
    private readonly float[] keyGlow = new float[4];

    private SSCSimfile simfile;
    private uint chartIndex;

    // Countdown
    private int countdownNumber = 3;
    private TimeSpan countdownTimer = TimeSpan.FromSeconds(1);
    private bool countdownOver => countdownNumber <= 0;

    // Judgments
    private readonly float perfectWindow = 0.05f;
    private readonly float greatWindow = 0.1f;
    private readonly float goodWindow = 0.2f;

    private readonly List<ActiveJudgment> judgments = new();

    // Combo & Score
    private int combo = 0;
    private int score = 0;

    // Active notes tracking per column
    private readonly Queue<int>[] columnQueues = new Queue<int>[4];

    public PlayerScene(SSCSimfile simfile, uint chartIndex)
    {
        this.simfile = simfile;
        this.chartIndex = chartIndex;
        audio = new Audio.Audio(simfile.Music!);

        for (int i = 0; i < 4; i++)
            columnQueues[i] = new Queue<int>();

        // Fill queues for each column
        for (int i = 0; i < simfile.Charts[chartIndex].Notes.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (simfile.Charts[chartIndex].Notes[i].row[j] != SSCNoteType.Empty)
                    columnQueues[j].Enqueue(i);
            }
        }
    }

    public void OnInput(in InputEvent input)
    {
        if (!countdownOver) return;
        if (input.Type != InputEvent.EventType.Key) return;

        var k = input.Key;
        if (k.Repeat) return;

        switch ((SDL.Keycode)k.Key)
        {
            case SDL.Keycode.A: keyGlow[0] = 1f; JudgeColumn(0); break;
            case SDL.Keycode.S: keyGlow[1] = 1f; JudgeColumn(1); break;
            case SDL.Keycode.D: keyGlow[2] = 1f; JudgeColumn(2); break;
            case SDL.Keycode.F: keyGlow[3] = 1f; JudgeColumn(3); break;
        }
    }

    public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
    {
        var area = renderer.RenderArea;
        float columnWidth = 200f;
        float noteHeight = 10f;
        float hitLineY = 100 + (area.H - 200);

        // Countdown
        if (!started)
        {
            startDelay -= deltaTime;
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

            DrawCountdown(renderer, area);
            return;
        }

        float currentBeat = BeatAt((float)audio.PlayBackPosition.TotalSeconds + simfile.Offset);

        // -------------------------
        // Background
        renderer.DrawRectFilled(
            new SDL.FRect { X = 0, Y = 0, W = area.W, H = area.H },
            new SDL.FColor(0.08f, 0.08f, 0.1f, 1f)
        );

        // -------------------------
        // Columns + glow
        var baseColor = new SDL.FColor(0.15f, 0.15f, 0.2f, 1f);
        for (byte i = 0; i < 4; i++)
        {
            var glowColor = new SDL.FColor(0.35f, 0.35f, 0.6f, 1f);
            float glow = keyGlow[i];
            SDL.FColor col = LerpColor(baseColor, glowColor, glow);

            renderer.DrawRectFilled(
                new SDL.FRect { X = columnWidth * i, Y = 100, W = columnWidth, H = area.H - 200 },
                col
            );

            renderer.DrawRectFilled(
                new SDL.FRect { X = columnWidth * i + columnWidth - 2, Y = 100, W = 2, H = area.H - 200 },
                new SDL.FColor(0.05f, 0.05f, 0.05f, 1f)
            );

            keyGlow[i] = Math.Max(0f, glow - (float)deltaTime.TotalSeconds * 2f);
        }

        // -------------------------
        // Hit line
        renderer.DrawRectFilled(
            new SDL.FRect { X = 0, Y = hitLineY, W = columnWidth * 4, H = 4 },
            new SDL.FColor(1f, 1f, 1f, 1f)
        );

        // -------------------------
        // Key caps
        string[] keyLabels = { "A", "S", "D", "F" };
        for (int i = 0; i < keyLabels.Length; i++)
        {
            renderer.DrawRectFilled(
                new SDL.FRect { X = columnWidth * i + 20, Y = hitLineY + 20, W = columnWidth - 40, H = 60 },
                new SDL.FColor(0.25f, 0.25f, 0.3f, 1f)
            );

            renderer.DrawText(
                keyLabels[i],
                columnWidth * i + columnWidth / 2 - 6,
                hitLineY + 35,
                new SDL.FColor(1f, 1f, 1f, 1f)
            );
        }

        // -------------------------
        // Notes + automatic misses
        float distanceBetweenBeats = 400f;
        var noteColor = new SDL.FColor(1f, 1f, 0f, 1f);
        var mineColor = new SDL.FColor(1f, 0f, 0f, 1f);
        var liftColor = new SDL.FColor(0f, 1f, 0f, 1f);

        for (int i = 0; i < simfile.Charts[chartIndex].Notes.Length; i++)
        {
            ref var note = ref simfile.Charts[chartIndex].Notes[i];
            for (int j = 0; j < 4; j++)
            {
                if (note.row[j] == SSCNoteType.Empty) continue;

                float noteY =
#if FLIP_SCROLL_DIRECTION
                    (currentBeat - note.beat.Value)
#else
                    (note.beat.Value - currentBeat)
#endif
                    * distanceBetweenBeats + 100;

                if (noteY > area.H) continue;

                // Draw note
                renderer.DrawRectFilled(
                    new SDL.FRect { X = columnWidth * j, Y = noteY, W = columnWidth, H = noteHeight },
                    note.row[j] switch
                    {
                        SSCNoteType.Mine => mineColor,
                        SSCNoteType.Lift => liftColor,
                        _ => noteColor
                    }
                );

                // Automatic miss
                if (noteY > hitLineY + goodWindow * distanceBetweenBeats && columnQueues[j].Count > 0 && columnQueues[j].Peek() == i)
                {
                    AddJudgment("Miss", new SDL.FColor(1f, 0f, 0f, 1f));
                    combo = 0;
                    columnQueues[j].Dequeue();
                }
            }
        }

        // -------------------------
        // Render judgments
        float judgmentYFixed = hitLineY - 50;
        for (int i = judgments.Count - 1; i >= 0; i--)
        {
            var j = judgments[i];
            renderer.DrawText(j.text, area.W / 2 - 30, j.y, j.color);
            j.timer -= (float)deltaTime.TotalSeconds;
            if (j.timer <= 0) judgments.RemoveAt(i);
            else judgments[i] = j;
        }

        // Draw combo & score
        if (combo > 1)
            renderer.DrawText($"COMBO {combo}", area.W / 2 - 50, judgmentYFixed - 40, new SDL.FColor(1f, 0.8f, 0f, 1f));
        renderer.DrawText($"SCORE {score}", area.W / 2 - 50, judgmentYFixed - 70, new SDL.FColor(1f, 1f, 1f, 1f));
    }

    private void DrawCountdown(WindowRenderer renderer, SDL.FRect area)
    {
        string text = countdownNumber > 0 ? countdownNumber.ToString() : "GO!";
        float x = area.W / 2 - 20;
        float y = area.H / 2 - 20;
        renderer.DrawText(text, x, y, new SDL.FColor(1f, 1f, 1f, 1f));
    }

    private void JudgeColumn(int column)
    {
        if (columnQueues[column].Count == 0) return;

        int noteIndex = columnQueues[column].Peek();
        ref var note = ref simfile.Charts[chartIndex].Notes[noteIndex];

        float currentBeat = BeatAt((float)audio.PlayBackPosition.TotalSeconds + simfile.Offset);
        float delta = Math.Abs(note.beat.Value - currentBeat);

        string result = "Miss";
        SDL.FColor color = new SDL.FColor(1f, 0f, 0f, 1f);
        int points = 0;

        if (delta <= perfectWindow) { result = "Perfect"; color = new SDL.FColor(1f, 1f, 0f, 1f); points = 100; }
        else if (delta <= greatWindow) { result = "Great"; color = new SDL.FColor(0f, 1f, 0f, 1f); points = 70; }
        else if (delta <= goodWindow) { result = "Good"; color = new SDL.FColor(0f, 0.7f, 1f, 1f); points = 50; }
        else return; // Too early, do nothing

        combo++;
        score += points;
        AddJudgment(result, color);

        columnQueues[column].Dequeue();
    }

    private void AddJudgment(string text, SDL.FColor color)
    {
        judgments.Add(new ActiveJudgment { text = text, timer = 1f, color = color, y = 0 });
    }

    private float BeatAt(float seconds)
    {
        float secondsPassed = 0f;
        float beatsPassed = 0f;

        for (var i = 0; i < simfile.BPMs.Length; i++)
        {
            var rem = seconds - secondsPassed;
            ref var bpm = ref simfile.BPMs[i];

            if (i == simfile.BPMs.Length - 1)
                return beatsPassed + rem / 60 * bpm.BPM;

            ref var next = ref simfile.BPMs[i + 1];
            float durationSec = (next.FromBeat - bpm.FromBeat) / bpm.BPM * 60f;

            if (rem < durationSec)
                return beatsPassed + rem / 60 * bpm.BPM;

            secondsPassed += durationSec;
            beatsPassed += durationSec / 60 * bpm.BPM;
        }

        throw new UnreachableException();
    }

    private SDL.FColor LerpColor(SDL.FColor a, SDL.FColor b, float t)
    {
        return new SDL.FColor(
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t,
            1f
        );
    }

    private struct ActiveJudgment
    {
        public string text;
        public float timer;
        public SDL.FColor color;
        public float y;
    }
}
