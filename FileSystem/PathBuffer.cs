
using System.Runtime.CompilerServices;

namespace wah.FileSystem
{
	[InlineArray(Length)]
	internal struct PathBuffer
	{
		public const byte Length = 255;
		private char c;
	}
}
