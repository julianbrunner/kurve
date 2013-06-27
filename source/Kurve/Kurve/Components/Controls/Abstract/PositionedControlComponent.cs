using System;
using Kurve.Interface;
using Krach.Basics;
using Kurve.Curves;

namespace Kurve.Component
{
	delegate void PositionedLengthInsertion(double position, double length);

	abstract class PositionedControlComponent : LengthControlComponent
	{
		const double dragThreshold = 10;

		readonly CurveComponent curveComponent;

		bool selected = false;
		bool dragging = false;
		bool isMouseDown = false;
		Vector2Double mouseDownPosition = Vector2Double.Origin;

		public event PositionedLengthInsertion InsertLength;

		public BasicSpecification BasicSpecification { get { return curveComponent.BasicSpecification; } }
		public Curve Curve { get { return curveComponent.Curve; } }
		public abstract double Position { get; }
		public bool Selected { get { return selected; } }
		public bool Dragging { get { return dragging; } }
		public bool IsMouseDown { get { return isMouseDown; } }

		public PositionedControlComponent(Component parent, CurveComponent curveComponent) : base(parent)
		{
			if (curveComponent == null) throw new ArgumentNullException("curveComponent");

			this.curveComponent = curveComponent;
		}

		public override void MouseDown(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (Contains(mousePosition) && mouseButton == MouseButton.Left)
			{
				isMouseDown = true;
				mouseDownPosition = mousePosition;

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
			if (isMouseDown && (mousePosition - mouseDownPosition).Length >= dragThreshold)
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

