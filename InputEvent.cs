using SDL3;
using System.Runtime.InteropServices;

namespace wah
{
	internal readonly struct InputEvent
	{
		public enum EventType
		{
			Key,
			Mouse,
		}

		public InputEvent(in SDL.KeyboardEvent key)
		{
			m_Type = EventType.Key;
			m_EventUnion.key = key;
		}

		public InputEvent(in SDL.MouseButtonEvent mouse)
		{
			m_Type = EventType.Mouse;
			m_EventUnion.mouse = mouse;
		}

		public EventType Type => m_Type;
		public readonly SDL.KeyboardEvent Key => m_EventUnion.key;
		public readonly SDL.MouseButtonEvent Mouse => m_EventUnion.mouse;

		[StructLayout(LayoutKind.Explicit)]
		private struct EventUnion
		{
			[FieldOffset(0)] public SDL.KeyboardEvent key;
			[FieldOffset(0)] public SDL.MouseButtonEvent mouse;
		}

		private readonly EventType m_Type;
		private readonly EventUnion m_EventUnion;
	}
}