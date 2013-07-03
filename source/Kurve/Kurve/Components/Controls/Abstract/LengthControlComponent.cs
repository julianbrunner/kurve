using System;
using Kurve.Interface;
using Krach.Basics;
using Kurve.Curves;

namespace Kurve.Component
{
	abstract class LengthControlComponent : Component
	{
		bool isShiftDown = false;
		bool isFineGrained = false;

		public bool IsShiftDown { get { return isShiftDown; } }

		public LengthControlComponent(Component parent) : base(parent) { }

		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (isShiftDown)
			{
				double stepSize = isFineGrained ? 1 : 10;

				double length;

				switch (scrollDirection)
				{
					case ScrollDirection.Up: length = -stepSize; break;
					case ScrollDirection.Down: length = +stepSize; break;
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
			if (key == Key.Alt)
			{
				isFineGrained = true;

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
			if (key == Key.Alt)
			{
				isFineGrained = false;

				Changed();
			}

			base.KeyUp(key);
		}

		protected abstract void OnInsertLength(double length);
	}
}

