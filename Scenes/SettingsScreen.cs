using System;
using SDL3;
using wah.Util;

namespace wah.Scenes
{
    internal class SettingsScreen : IScene
    {
        private MouseState m_State;

        // Static so the value persists across all scenes
        public static float NoteSpeed = 1.0f;

        // Hover animation 
        private float hoverPlus = 0f;
        private float hoverMinus = 0f;

        public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
        {
            var area = renderer.RenderArea;

            //  Background (Dark Blue/Grey)
            renderer.DrawRectFilled(
                new SDL.FRect { X = 0, Y = 0, W = area.W, H = area.H },
                new SDL.FColor(0.05f, 0.05f, 0.1f, 1f)
            );

            float centerX = area.W / 2;
            float centerY = area.H / 2;
            SDL.FColor white = new SDL.FColor(1f, 1f, 1f, 1f);
            SDL.FColor black = new SDL.FColor(0f, 0f, 0f, 1f);
            SDL.FColor baseBtnColor = new SDL.FColor(0.8f, 0.8f, 0.8f, 1f);
            SDL.FColor hoverBtnColor = new SDL.FColor(1f, 0.9f, 0.3f, 1f);

            // Title Text
            renderer.DrawText("SETTINGS", centerX - 60, 50, white);

            // Note Speed UI logic
            renderer.DrawText($"NOTE SPEED: {NoteSpeed:F1}x", centerX - 100, centerY, white);

            // Minus Button Rect
            SDL.FRect minusBtn = new SDL.FRect { X = centerX - 150, Y = centerY - 5, W = 40, H = 40 };
            bool hoveringMinus = MouseOver(minusBtn);
            hoverMinus = hoveringMinus
                ? Math.Min(hoverMinus + (float)deltaTime.TotalSeconds * 5f, 1f)
                : Math.Max(hoverMinus - (float)deltaTime.TotalSeconds * 5f, 0f);

            renderer.DrawRectFilled(minusBtn, LerpColor(baseBtnColor, hoverBtnColor, hoverMinus));
            renderer.DrawText("-", minusBtn.X + 15, minusBtn.Y + 10, black);

            if (hoveringMinus && m_State.leftClickedThisFrame)
            {
                NoteSpeed = Math.Max(0.1f, NoteSpeed - 0.1f);
            }

            // Plus Button Rect
            SDL.FRect plusBtn = new SDL.FRect { X = centerX + 110, Y = centerY - 5, W = 40, H = 40 };
            bool hoveringPlus = MouseOver(plusBtn);
            hoverPlus = hoveringPlus
                ? Math.Min(hoverPlus + (float)deltaTime.TotalSeconds * 5f, 1f)
                : Math.Max(hoverPlus - (float)deltaTime.TotalSeconds * 5f, 0f);

            renderer.DrawRectFilled(plusBtn, LerpColor(baseBtnColor, hoverBtnColor, hoverPlus));
            renderer.DrawText("+", plusBtn.X + 12, plusBtn.Y + 10, black);

            if (hoveringPlus && m_State.leftClickedThisFrame)
            {
                NoteSpeed = Math.Min(10.0f, NoteSpeed + 0.1f);
            }

            
            renderer.DrawText("Press ESC to return", 20, area.H - 40, new SDL.FColor(0.6f, 0.6f, 0.6f, 1f));

            
            m_State.leftClickedThisFrame = false;
        }

        public void OnInput(in InputEvent input)
        {
            
            if (input.Type == InputEvent.EventType.Mouse)
            {
                var m = input.Mouse;
                m_State.pos = new SDL.FPoint { X = m.X, Y = m.Y };
                m_State.leftClickedThisFrame = (m.Button == (byte)SDL.MouseButtonFlags.Left) && m.Down;
            }

            
            if (input.Type == InputEvent.EventType.Key && input.Key.Down)
            {
                if (input.Key.Raw == (ushort)SDL.Scancode.Escape)
                {
                    SceneManager.Current = new TitleScreen();
                }
            }
        }

        
        private bool MouseOver(SDL.FRect rect) =>
            m_State.pos.X >= rect.X && m_State.pos.X <= rect.X + rect.W &&
            m_State.pos.Y >= rect.Y && m_State.pos.Y <= rect.Y + rect.H;

        private SDL.FColor LerpColor(SDL.FColor a, SDL.FColor b, float t) =>
            new SDL.FColor(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t, 1f);

        private struct MouseState
        {
            public SDL.FPoint pos;
            public bool leftClickedThisFrame;
        }
    }
}