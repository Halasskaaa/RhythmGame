using SDL3;

namespace wah.Util
{
    internal static class FRectUtil
    {
        public static bool ContainsPoint(in this SDL.FRect rect, SDL.FPoint p)
        {
            return rect.X <= p.X && rect.Y <= p.Y && rect.X + rect.W >= p.X && rect.Y + rect.H >= p.Y;
        }
    }
}
