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

		bool isSelected = false;
		bool isDragging = false;
		bool isLeftMouseDown = false;
		bool isRightMouseDown = false;
		Vector2Double dragVector = Vector2Double.Origin;
		Vector2Double accumulatedDragVector = Vector2Double.Origin;
		Vector2Double mouseDownPosition = Vector2Double.Origin;
		Vector2Double lastMousePosition = Vector2Double.Origin;

		public event PositionedLengthInsertion InsertLength;
		public event Action<PositionedControlComponent> SelectionChanged;

		public BasicSpecification BasicSpecification { get { return curveComponent.BasicSpecification; } }
		public Curve Curve { get { return curveComponent.Curve; } }
		public abstract double Position { get; }
		public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
		public bool IsDragging { get { return isDragging; } }
		public Vector2Double DragVector { get { return dragVector; } }
		public Vector2Double AccumulatedDragVector { get { return accumulatedDragVector; } }
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
				if ((mousePosition - mouseDownPosition).Length <= dragThreshold) {
					isSelected = !isSelected;
					OnSelectionChanged();
				}
				if (mouseButton == MouseButton.Left) isLeftMouseDown = false;
				if (mouseButton == MouseButton.Right) isRightMouseDown = false;
				isDragging = false;
				dragVector = Vector2Double.Origin;
				accumulatedDragVector = Vector2Double.Origin;

				Changed();
			}
			
			base.MouseUp(mousePosition, mouseButton);
		}
		public override void MouseMove(Vector2Double mousePosition)
		{
			if (isLeftMouseDown)
			{
				isDragging = true;
				dragVector = mousePosition - lastMousePosition;
				accumulatedDragVector += dragVector;

				Changed();
			}

			lastMousePosition = mousePosition;

			base.MouseMove(mousePosition);
		}

		public abstract bool Contains(Vector2Double point);

		protected override void OnInsertLength(double length)
		{
			if (!isSelected) return;

			if (InsertLength != null) InsertLength(Position, length);
		}
		protected void OnSelectionChanged() 
		{
			if (SelectionChanged != null) SelectionChanged(this);
		}
	}
}

