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
		bool isLeftMouseDown = false;
		bool isRightMouseDown = false;
		Vector2Double mouseDownPosition = Vector2Double.Origin;
		Vector2Double lastMousePosition = Vector2Double.Origin;
		Vector2Double dragVector = Vector2Double.Origin;

		public event PositionedLengthInsertion InsertLength;

		public BasicSpecification BasicSpecification { get { return curveComponent.BasicSpecification; } }
		public Curve Curve { get { return curveComponent.Curve; } }
		public abstract double Position { get; }
		public bool Selected { get { return selected; } }
		public bool Dragging { get { return dragging; } }
		public Vector2Double DragVector { get { return dragVector; } }
		public bool IsLeftMouseDown { get { return isLeftMouseDown; } }
		public bool IsRightMouseDown { get { return isRightMouseDown; } }

		public PositionedControlComponent(Component parent, CurveComponent curveComponent) : base(parent)
		{
			if (curveComponent == null) throw new ArgumentNullException("curveComponent");

			this.curveComponent = curveComponent;
		}

		public override void MouseDown(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (Contains(mousePosition) && (mouseButton == MouseButton.Left || mouseButton == MouseButton.Right))
			{
				if (mouseButton == MouseButton.Left) isLeftMouseDown = true;
				if (mouseButton == MouseButton.Right) isRightMouseDown = true;
				
				mouseDownPosition = mousePosition;

				Changed();
			}

			base.MouseDown(mousePosition, mouseButton);
		}
		public override void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if ((isLeftMouseDown || isRightMouseDown) && (mouseButton == MouseButton.Left || mouseButton == MouseButton.Right))
			{
				if ((mousePosition - mouseDownPosition).Length <= dragThreshold) selected = !selected;
				if (mouseButton == MouseButton.Left) isLeftMouseDown = false;
				if (mouseButton == MouseButton.Right) isRightMouseDown = false;
				dragging = false;
				dragVector = Vector2Double.Origin;

				Changed();
			}
			
			base.MouseUp(mousePosition, mouseButton);
		}
		public override void MouseMove(Vector2Double mousePosition)
		{
			if (isLeftMouseDown)
			{
				dragging = true;
				dragVector = mousePosition - lastMousePosition;

				Changed();
			}

			lastMousePosition = mousePosition;

			base.MouseMove(mousePosition);
		}

		public abstract bool Contains(Vector2Double point);

		protected override void OnInsertLength(double length)
		{
			if (!selected) return;

			if (InsertLength != null) InsertLength(Position, length);
		}
	}
}

