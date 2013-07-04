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
		bool isFineGrained = false; // TODO replace fineGrained with SlowdownFactor

		public bool IsShiftDown { get { return isShiftDown; } }
		public bool IsWindowsDown { get { return isWindowsDown; } }
		public bool IsControlDown { get { return isControlDown; } }
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
				isFineGrained = false;

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

