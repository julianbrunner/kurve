using System;
using Kurve.Interface;
using Krach.Basics;
using Kurve.Curves;

namespace Kurve.Component
{
	abstract class LengthControlComponent : Component
	{
		bool isShiftDown = false;
		bool isWindowsDown = false;
		bool isControlDown = false;
		double slowDownFactor = 1;

		public bool IsShiftDown { get { return isShiftDown; } }
		public bool IsWindowsDown { get { return isWindowsDown; } }
		public bool IsControlDown { get { return isControlDown; } }
		public double SlowDownFactor { get { return slowDownFactor; } }

		public LengthControlComponent(Component parent) : base(parent) { }

		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (isShiftDown)
			{
				double stepSize = 10 * slowDownFactor;

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
				slowDownFactor = 0.1;

				Changed();
			}
			if (key == Key.Windows) 
			{
				isWindowsDown = true;

				Changed();
			}
			if (key == Key.Control) 
			{
				isControlDown = true;

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
				slowDownFactor = 1.0;

				Changed();
			}
			if (key == Key.Windows) 
			{
				isWindowsDown = false;

				Changed();
			}
			if (key == Key.Control) 
			{
				isControlDown = false;

				Changed();
			}

			base.KeyUp(key);
		}

		protected abstract void OnInsertLength(double length);
	}
}

