#define FLIP_SCROLL_DIRECTION

using SDL3;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using wah.Chart.SSC;

namespace wah.Scenes;

internal class PlayerScene(SSCSimfile simfile, uint chartIndex) : IScene
{
	private bool started;
	private Audio.Audio audio = new Audio.Audio(simfile.Music!);
	private TimeSpan startDelay = TimeSpan.FromSeconds(3);
	private ColumnStates columnStates;
	private JudgeState[,] judgeStates = new JudgeState[simfile.Charts[chartIndex].Notes.Length, SSCNoteRow.Length];

	// Countdown
	private int countdownNumber = 3;
	private TimeSpan countdownTimer = TimeSpan.FromSeconds(1);
	private bool countdownOver => countdownNumber <= 0;

	// Judgments
	private const float perfectWindow = 0.2f;
	private const float goodWindow = 0.4f;

	private readonly SDL.FColor defaultGlowColor = new(0.35f, 0.35f, 0.6f, 1f);

	// Combo & Score
	private int combo = 0;
	private int score = 0;

	public void OnInput(in InputEvent input)
	{
		if (!countdownOver) return;
		if (input.Type != InputEvent.EventType.Key) return;

		var k = input.Key;
		if (k.Repeat) return;

		ref var state = ref Unsafe.NullRef<ColumnInputState>();

		switch (k.Key)
		{
			case SDL.Keycode.A: state = ref columnStates[0]; break;
			case SDL.Keycode.S: state = ref columnStates[1]; break;
			case SDL.Keycode.D: state = ref columnStates[2]; break;
			case SDL.Keycode.F: state = ref columnStates[3]; break;
		}

		if (Unsafe.IsNullRef(ref state)) return;

		state.pressing = k.Down;
		state.pressedThisFrame = state.pressing;
		state.relasedThisFrame = !state.pressing;
		state.glowValue = 1;
		state.glowColor = defaultGlowColor;
	}

	public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
	{
		const float columnWidth = 200f;
		const float noteHeight = 10f;
		var area = renderer.RenderArea;
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
			ref var state = ref columnStates[i];
			SDL.FColor col = LerpColor(baseColor, state.glowColor, state.glowValue);

			renderer.DrawRectFilled(
				new SDL.FRect { X = columnWidth * i, Y = 100, W = columnWidth, H = area.H - 200 },
				col
			);

			renderer.DrawRectFilled(
				new SDL.FRect { X = columnWidth * i + columnWidth - 2, Y = 100, W = 2, H = area.H - 200 },
				new SDL.FColor(0.05f, 0.05f, 0.05f, 1f)
			);

			state.glowValue = Math.Max(0f, state.glowValue - (float)deltaTime.TotalSeconds * 2f);
		}

		// -------------------------
		// Hit line
		renderer.DrawRectFilled(
			new SDL.FRect { X = 0, Y = hitLineY, W = columnWidth * 4, H = 4 },
			new SDL.FColor(1f, 1f, 1f, 1f)
		);

		// -------------------------
		// Key caps
		const string KL0 = "A";
		const string KL1 = "S";
		const string KL2 = "D";
		const string KL3 = "F";
		ReadOnlySpan<string> keyLabels = [KL0, KL1, KL2, KL3];

		for (byte i = 0; i < keyLabels.Length; i++)
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
		float distanceBetweenBeats = 400;
		var noteColor = new SDL.FColor(1f, 1f, 0f, 1f);
		var mineColor = new SDL.FColor(1f, 0f, 0f, 1f);
		var liftColor = new SDL.FColor(0f, 1f, 0f, 1f);

		for (int i = 0; i < simfile.Charts[chartIndex].Notes.Length; i++)
		{
			ref var note = ref simfile.Charts[chartIndex].Notes[i];

			for (int j = 0; j < 4; j++)
			{
				if (note.row[j] == SSCNoteType.Empty) continue;

				ref var judgeState = ref judgeStates[i, j];
				if (judgeState.judgement != Judgement.Unjudged) continue;


				float noteY =
#if FLIP_SCROLL_DIRECTION
					area.H + (currentBeat - note.beat.Value)
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

				var diff = currentBeat - note.beat.Value;
				var aDiff = Math.Abs(diff);

				if (diff > goodWindow)
				{
					judgeState.judgement = Judgement.Miss;
					combo = 0;
				}
				else if (columnStates[j].pressedThisFrame)
				{
					const float badIgnoreDiff = 1 - goodWindow;
					if (aDiff > goodWindow && aDiff < badIgnoreDiff)
					{
						judgeState.judgement = Judgement.Bad;
						combo = 0;
						columnStates[j].pressedThisFrame = false;
						columnStates[j].glowColor = new SDL.FColor(1f, 0f, 0f, 1f);
					}
					else if (aDiff <= goodWindow && aDiff > perfectWindow)
					{
						judgeState.judgement = diff > 0 ? Judgement.GoodEarly : Judgement.GoodLate;
						columnStates[j].pressedThisFrame = false;
						columnStates[j].glowColor = new SDL.FColor(0f, 1f, 0f, 1f);
						combo++;
						score++;
					}
					else
					{
						judgeState.judgement = Judgement.Perfect;
						columnStates[j].pressedThisFrame = false;
						columnStates[j].glowColor = new SDL.FColor(0f, 0f, 1f, 1f);
						combo++;
						score++;
					}
				}
			}
		}

		// -------------------------
		// Render judgments
		float judgmentYFixed = hitLineY - 50;
		//for (var i = judgeStates.GetLength(0) - 1; i >= 0; --i)
		//{
		//    var @break = false;
		//    for (var j = 0; j < judgeStates.GetLength(1); ++j)
		//    {
		//        var state = judgeStates[i, j];

		//        if (state.judgement == Judgement.Unjudged) continue;
		//        @break = true;
		//    }

		//    if (@break) break;
		//}
		//for (int i = judgments.Count - 1; i >= 0; i--)
		//{
		//    var j = judgments[i];
		//    renderer.DrawText(j.text, area.W / 2 - 30, j.y, j.color);
		//    j.timer -= (float)deltaTime.TotalSeconds;
		//    if (j.timer <= 0) judgments.RemoveAt(i);
		//    else judgments[i] = j;
		//}

		// Draw combo & score
		if (combo > 1)
			renderer.DrawText($"COMBO {combo}", area.W / 2 - 50, judgmentYFixed - 40, new SDL.FColor(1f, 0.8f, 0f, 1f));
		renderer.DrawText($"SCORE {score}", area.W / 2 - 50, judgmentYFixed - 70, new SDL.FColor(1f, 1f, 1f, 1f));

		for (byte i = 0; i < ColumnStates.Length; ++i)
		{
			columnStates[i].relasedThisFrame = false;
			columnStates[i].pressedThisFrame = false;
		}
	}

	private void DrawCountdown(WindowRenderer renderer, SDL.FRect area)
	{
		string text = countdownNumber > 0 ? countdownNumber.ToString() : "GO!";
		float x = area.W / 2 - 20;
		float y = area.H / 2 - 20;
		renderer.DrawText(text, x, y, new SDL.FColor(1f, 1f, 1f, 1f));
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

	[InlineArray(Length)]
	private struct KeyGlowStates
	{
		public const byte Length = SSCNoteRow.Length;
		public float state;
	}

	[InlineArray(Length)]
	private struct ColumnStates
	{
		public const byte Length = SSCNoteRow.Length;
		private ColumnInputState _;
	}

	private struct ColumnInputState
	{
		public SDL.FColor glowColor;
		public float glowValue;
		public bool pressing, pressedThisFrame, relasedThisFrame;
	}

	private enum Judgement
	{
		Unjudged = 0,
		Miss = 1,
		GoodLate = 3,
		Perfect = 5,
		GoodEarly = 4,
		Bad = 2
	}

	private struct JudgeState
	{
		public Judgement judgement;
	}
}
