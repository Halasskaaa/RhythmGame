using System;
using wah;

namespace wah
{
    internal static class SceneManager
    {
        private static IScene  m_Current = new NoopScene();

        public static IScene Current
        {
            get => m_Current;
            set => m_Current = value;
        }
    }
}

file class NoopScene : IScene
{
    public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer) { }

    public void OnInput(in InputEvent input) { }
}
