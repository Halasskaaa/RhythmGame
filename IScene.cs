using System;

namespace wah
{
	internal interface IScene
	{
		public void OnInput(in InputEvent input);
		public void OnDrawFrame(TimeSpan deltaTime, ref WindowRenderer renderer);
	}
}
