//#define FLIP_SCROLL_DIRECTION

using System.Diagnostics;
using SDL3;
using wah.Chart.SSC;

namespace wah.Scenes;

internal class PlayerScene(SSCSimfile simfile, uint chartIndex) : IScene
{
	private bool started;
	private Audio.Audio audio = new Audio.Audio(simfile.Music!);
	private TimeSpan startDelay = TimeSpan.FromSeconds(2);

	public void OnInput(in InputEvent input) { }

	public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
	{
		startDelay -= deltaTime;
		if (!started && startDelay.Ticks <= 0)
		{
			started = true;
			audio.Play();
		}

		var area = renderer.RenderArea;
		const float columnWidth = 200;

		var columnRect = new SDL.FRect { W = columnWidth, H = area.H - 200, X = 0, Y = 100 };
		var columnColor = new SDL.FColor { R = 0.3f, G = 0.3f, B = 0.3f, A = 1.0f };

		for (byte i = 0; i < SSCNoteRow.Length; ++i)
		{
			renderer.DrawRectFilled(columnRect with { X = columnWidth * i }, in columnColor);
		}

		const float distanceBetweenBeats = 400;
		const float noteHeight = 10;
		const float beatLineHeight = 2;
		var noteColor = new SDL.FColor { R = 1.0f, G = 1.0f, B = 0.0f, A = 1.0f };
		var beatLineColor = new SDL.FColor { R = 0.6f, G = 0.6f, B = 0.6f, A = 1.0f };
		var mineNoteColor = new SDL.FColor { R = 1.0f, G = 0.0f, B = 0.0f, A = 1.0f };
		var liftNoteColor = new SDL.FColor { R = 0.0f, G = 1.0f, B = 0.0f, A = 1.0f };

		var currentBeat = BeatAt((float)(started ? audio.PlayBackPosition.TotalSeconds : -startDelay.TotalSeconds) +
								 simfile.Offset);
		Span<bool> holdDrawStates = stackalloc bool[SSCNoteRow.Length];
		var prevMeasure = -1;

		for (var i = 0; i < simfile.Charts[chartIndex].Notes.Length; ++i)
		{
			ref var measureEntry = ref simfile.Charts[chartIndex].Notes[i];

			if (measureEntry.measure != prevMeasure)
			{
				prevMeasure = measureEntry.measure;
				Console.WriteLine(measureEntry.measure);

				renderer.DrawRectFilled(new SDL.FRect
				{
					W = columnWidth * SSCNoteRow.Length,
					H = beatLineHeight,
					X = 0,
					Y = (measureEntry.measure - currentBeat) * distanceBetweenBeats + 100
				},
										beatLineColor);

				renderer.DrawText(measureEntry.measure.ToString(),
								  columnWidth * SSCNoteRow.Length,
								  (measureEntry.measure - currentBeat) * distanceBetweenBeats + 100,
								  mineNoteColor);
			}

			var time =
				// (float)measureEntry.numerator / measureEntry.denominator;
				measureEntry.beat.Value;
			//measureEntry.measure + (float)measureEntry.numerator / measureEntry.denominator;
			var y =
#if FLIP_SCROLL_DIRECTION
				(currentBeat - time)
#else
				(time - currentBeat)
#endif
				* distanceBetweenBeats;

			if (y + 100 > renderer.RenderArea.H) break;

			for (byte j = 0; j < SSCNoteRow.Length; ++j)
			{
				var note = measureEntry.row[j];
				if (note == SSCNoteType.Empty) continue;

				if (note == SSCNoteType.HoldHead || note == SSCNoteType.RollHead)
				{
					for (var k = i + 1; k < simfile.Charts[chartIndex].Notes.Length; ++k)
					{
						if (simfile.Charts[chartIndex].Notes[k].row[j] == SSCNoteType.Tail)
						{
							renderer.DrawRectFilled(new SDL.FRect
							{
								W = columnWidth,
								H = (simfile.Charts[chartIndex].Notes[k].beat.Value - time) * distanceBetweenBeats,
								X = columnWidth * j,
								Y = y + 100
							},
											noteColor with { A = 0.001f });
							break;
						}
					}
				}

				renderer.DrawRectFilled(new SDL.FRect
				{
					W = columnWidth,
					H = noteHeight,
					X = columnWidth * j,
					Y = y + 100
				},
										note switch
										{
											SSCNoteType.Mine => mineNoteColor,
											SSCNoteType.Lift => liftNoteColor,
											_ => noteColor
										});
			}
		}
	}

	private float BeatAt(float seconds)
	{
		// bpm list should always have default bpm from 0 as the first entry
		var secondsPassed = 0f;
		var beatsPassed = 0f;
		for (var i = 0; i < simfile.BPMs.Length; ++i)
		{
			var rem = seconds - secondsPassed;

			ref var bpm = ref simfile.BPMs[i];
			// no next entry
			if (i == simfile.BPMs.Length - 1)
			{
				return beatsPassed + rem / 60 * bpm.BPM;
			}

			ref var next = ref simfile.BPMs[i + 1];
			var durationSec = (next.FromBeat - bpm.FromBeat) / bpm.BPM * 60;
			if (rem < durationSec)
			{
				return beatsPassed + rem / 60 * bpm.BPM;
			}

			secondsPassed += durationSec;
			beatsPassed += durationSec / 60 * bpm.BPM;
		}

		throw new UnreachableException();
	}
}
