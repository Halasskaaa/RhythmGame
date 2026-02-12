using SDL3;

namespace wah.Scenes
{
    internal class TitleScreen : IScene
    {
        private MouseState m_State;
        private float hoverAnimation = 0f; // 0..1 for smooth hover effect
        private float hoverAnimationSettings = 0f; // Added for settings button

        private static readonly MenuButton[] buttons =
        {
            new("START", 40, ()=> SceneManager.Current = new SongSelectScreen()),
            new("SETTINS", 60, ()=> SceneManager.Current = new SettingsScreen()),
            new("EXIT", 40, () => Environment.Exit(0))
        };

        public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
        {
            var area = renderer.RenderArea;

            // --------------------------
            // Background gradient
            for (int i = 0; i < area.H; i++)
            {
                float t = i / area.H;
                renderer.DrawRectFilled(
                    new SDL.FRect { X = 0, Y = i, W = area.W, H = 1 },
                    new SDL.FColor(0.05f + t * 0.1f, 0.05f + t * 0.1f, 0.1f + t * 0.15f, 1f)
                );
            }



            // --------------------------
            // Button dimensions
            const float ButtonW = 300f;
            const float ButtonH = 120f;
            const float ItemGap = 10;
            var centerX = area.W / 2;
            var centerY = area.H / 2;

            var y = centerY - ButtonH / 2 - 70;
            foreach (ref readonly var button in buttons.AsSpan())
            {
				SDL.FRect btnRect = new SDL.FRect
				{

					X = centerX - ButtonW / 2,
					Y = y,
					W = ButtonW,
					H = ButtonH
				};

				// Hover animation
				bool hovering = MouseOver(btnRect);
				hoverAnimation = hovering
					? Math.Min(hoverAnimation + (float)deltaTime.TotalSeconds * 5f, 1f)
					: Math.Max(hoverAnimation - (float)deltaTime.TotalSeconds * 5f, 0f);

				SDL.FColor baseColor = new SDL.FColor(0.8f, 0.8f, 0.8f, 1f);
				SDL.FColor hoverColor = new SDL.FColor(1f, 0.9f, 0.3f, 1f);

				// Lerp color based on hover
				SDL.FColor buttonColor = LerpColor(baseColor, hoverColor, hoverAnimation);

				renderer.DrawRectFilled(btnRect, buttonColor);

				renderer.DrawText(
					button.Text,
					centerX - button.TextWidth,
					btnRect.Y + (ButtonH / 2) - 10,
					new SDL.FColor(0f, 0f, 0f, 1f)
				);

				if (hovering && m_State.leftClickedThisFrame) button.OnClick();

				y += ButtonH + ItemGap;
			}

			m_State.leftClickedThisFrame = false;
        }

        public void OnInput(in InputEvent input)
        {
            if (input.Type != InputEvent.EventType.Mouse)
                return;

            var m = input.Mouse;
            m_State.pos = new SDL.FPoint { X = m.X, Y = m.Y };
            m_State.leftClickedThisFrame = m.Button == (byte)SDL.MouseButtonFlags.Left;
        }

        private bool MouseOver(SDL.FRect rect)
        {
            return m_State.pos.X >= rect.X && m_State.pos.X <= rect.X + rect.W
                && m_State.pos.Y >= rect.Y && m_State.pos.Y <= rect.Y + rect.H;
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

        private struct MouseState
        {
            public SDL.FPoint pos;
            public bool leftClickedThisFrame;
        }

        private readonly record struct MenuButton(string Text, float TextWidth, Action OnClick);
    }
}