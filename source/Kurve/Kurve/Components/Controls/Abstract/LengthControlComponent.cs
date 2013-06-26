using System;
using Kurve.Interface;
using Krach.Basics;
using Kurve.Curves;

namespace Kurve.Component
{
	abstract class LengthControlComponent : Component
	{
		bool isShiftDown = false;

		public bool IsShiftDown { get { return isShiftDown; } }

		public LengthControlComponent(Component parent) : base(parent) { }

		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (isShiftDown)
			{
				double length;

				switch (scrollDirection)
				{
					case ScrollDirection.Up: length = -10; break;
					case ScrollDirection.Down: length = +10; break;
					default: throw new ArgumentException();
				}

				OnInsertLength(length);

				Changed();
			}

			base.Scroll(scrollDirection);
		}
		public override void KeyDown(Key key)
		{
			if (key == Key.Shift)
			{
				isShiftDown = true;
			
				Changed();
			}

			base.KeyDown(key);
		}
		public override void KeyUp(Key key)
		{
			if (key == Key.Shift)
			{
				isShiftDown = false;

				Changed();
			}

			base.KeyUp(key);
		}

		public abstract void OnInsertLength(double length);
	}
}

