
using System;
using SDL3;
using wah.Util;

namespace wah.Scenes
{
    internal class TitleScreen : IScene
    {
        private MouseState m_State;
        public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer)
        {

            var area = renderer.RenderArea;
            const float ButtonW = 300;
            const float ButtonH = 200;

            var centerX = area.W / 2;
            var centerY = area.H / 2;


            // draw play button
            var btnRect = new SDL.FRect { X = centerX - ButtonW / 2, Y = centerY - ButtonH / 2, W = ButtonW, H = ButtonH };
            renderer.DrawRectFilled(btnRect, new(1f, 1f, 1f, 1f));

            if (btnRect.ContainsPoint(m_State.pos))
                SceneManager.Current = new SongSelectScreen();

            m_State.leftClickedThisFrame = false;
        }

        public void OnInput(in InputEvent input)
        {
            if (input.Type != InputEvent.EventType.Mouse) return;

            var m = input.Mouse;
            m_State.leftClickedThisFrame = m.Button == (byte)SDL.MouseButtonFlags.Left;
            m_State.pos = new() { X = m.X, Y = m.Y };
        }

        private struct MouseState
        {
            public SDL.FPoint pos;
            public bool leftClickedThisFrame;
        }
    }
}
