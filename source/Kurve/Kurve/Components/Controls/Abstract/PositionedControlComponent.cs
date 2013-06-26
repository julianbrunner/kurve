using System;
using Kurve.Interface;
using Krach.Basics;
using Kurve.Curves;

namespace Kurve.Component
{
	delegate void PositionedLengthInsertion(double position, double length);

	abstract class PositionedControlComponent : LengthControlComponent
	{
		Curve curve = null;
		bool selected = false;
		bool dragging = false;
		bool isMouseDown = false;
		
		public event PositionedLengthInsertion InsertLength;

		public Curve Curve
		{
			get { return curve; }
			set { curve = value; }
		}
		public abstract double Position { get; }
		public bool Selected { get { return selected; } }
		public bool Dragging { get { return dragging; } }
		public bool IsMouseDown { get { return isMouseDown; } }

		public PositionedControlComponent(Component parent) : base(parent) { }

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
		public override void MouseMove(Vector2Double mousePosition)
		{
			if (isMouseDown)
			{
				dragging = true;
			
				Changed();
			}

			base.MouseMove(mousePosition);
		}

		public override void OnInsertLength(double length)
		{
			if (!selected) return;

			if (InsertLength != null) InsertLength(Position, length);
		}

		public abstract bool Contains(Vector2Double point);
	}
}

