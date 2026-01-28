using SDL3;

namespace wah.Util
{
    internal static class AudioUtils
    {
        public static uint BitSize(this SDL.AudioFormat format) => (uint)format & SDL.AudioMaskBitSize;
    }
}
