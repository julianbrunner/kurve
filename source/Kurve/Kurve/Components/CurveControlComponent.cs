using System;
using Kurve.Interface;
using Krach.Basics;

namespace Kurve.Component
{
	delegate void InsertLength(double position, double length);

	abstract class CurveControlComponent : Component
	{
		bool selected;
		bool dragging;
		bool isMouseDown;
		bool isShiftDown;

		public event InsertLength InsertLength;

		public abstract double Position { get; }
		public bool Selected { get { return selected; } }
		public bool Dragging { get { return dragging; } }
		public bool IsMouseDown { get { return isMouseDown; } }
		public bool IsShiftDown { get { return isShiftDown; } }

		public CurveControlComponent(Component parent) : base(parent) 
		{
			this.selected = false;
			this.dragging = false;
			this.isMouseDown = false;
		}
	
		public abstract bool Contains(Vector2Double point);

		public override void MouseDown(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (Contains(mousePosition) && mouseButton == MouseButton.Left)
			{
				isMouseDown = true;

				Changed();
			}

			base.MouseDown(mousePosition, mouseButton);
		}
		public override void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (isMouseDown && mouseButton == MouseButton.Left)
			{
				if (!dragging) selected = !selected;
				isMouseDown = false;
				dragging = false;

				Changed();
			}
			
			base.MouseUp(mousePosition, mouseButton);
		}
		public override void MouseMove (Vector2Double mousePosition)
		{
			if (isMouseDown) 
			{
				dragging = true;
			
				Changed();
			}

			base.MouseMove(mousePosition);
		}
		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (selected && isShiftDown) {
				double insertedLength = 0;

				switch (scrollDirection) {
					case ScrollDirection.Down: insertedLength = -10; break;
					case ScrollDirection.Up: insertedLength = 10; break;
					default: break;
				}

				if (InsertLength != null) InsertLength(this.Position, insertedLength);
			}

			base.Scroll(scrollDirection);
		}
		public override void KeyDown(Key key)
		{
			if (key == Key.Shift) isShiftDown = true;

			base.KeyDown(key);
		}
		public override void KeyUp(Key key)
		{
			if (key == Key.Shift) isShiftDown = false;

			base.KeyUp(key);
		}
	}
}

