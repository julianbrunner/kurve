using System;
using Kurve.Interface;
using Cairo;
using Krach.Basics;
using Krach.Graphics;
using Kurve.Curves;
using Krach.Extensions;
using Krach.Maps.Scalar;
using Krach.Maps.Abstract;
using Krach.Maps;
using System.Linq;
using System.Collections.Generic;

namespace Kurve.Component
{
	class SegmentComponent : PositionedControlComponent
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);

		public event Action SpecificationChanged;
		public event Action<double> AddSpecification;

		readonly PositionedControlComponent leftComponent;
		readonly PositionedControlComponent rightComponent;

		Orthotope2Double Bounds { get { return new Orthotope2Double(Point - 0.5 * size, Point + 0.5 * size); } }
		int SegmentSegmentCount
		{
			get
			{
				double length = rightComponent.Position - leftComponent.Position;

				if (length == 0) return 1;

				return (int)(length * SegmentCount).Ceiling();
			}
		}

		public override double Position { get { return (leftComponent.Position + rightComponent.Position) / 2; } }
		public Vector2Double Point { get { return Curve == null ? Vector2Double.Origin : Curve.GetPoint(Position); } }

		public SegmentComponent(Component parent, CurveComponent curveComponent, PositionedControlComponent leftComponent, PositionedControlComponent rightComponent) : base(parent, curveComponent)
		{
			if (leftComponent == null) throw new ArgumentNullException("leftComponent");
			if (rightComponent == null) throw new ArgumentNullException("rightComponent");
		
			this.leftComponent = leftComponent;
			this.rightComponent = rightComponent;
		}

		public override void Draw(Context context)
		{
			if (Curve != null && IsSelected)
				foreach (Tuple<double, double> positions in Scalars.GetIntermediateValuesSymmetric(leftComponent.Position, rightComponent.Position, SegmentSegmentCount + 1).GetRanges())
					Drawing.DrawLine(context, Curve.GetPoint(positions.Item1), Curve.GetPoint(positions.Item2), 8, Colors.Green.ReplaceAlpha(0.3));

			base.Draw(context);
		}

		public override bool Contains(Vector2Double position)
		{
			if (Curve == null) return false;

			if (leftComponent is SpecificationComponent && (Curve.GetPoint(leftComponent.Position) - position).Length < 15) return false;
			if (rightComponent is SpecificationComponent && (Curve.GetPoint(rightComponent.Position) - position).Length < 15) return false;

			foreach (double testedPosition in Scalars.GetIntermediateValuesSymmetric(leftComponent.Position, rightComponent.Position, SegmentSegmentCount + 1))
				if ((Curve.GetPoint(testedPosition) - position).Length < 10)
					return true;

			return false;
		}

		public override void MouseMove(Vector2Double mousePosition)
		{
			base.MouseMove(mousePosition);

			if (IsDragging) 
			{
				foreach (SpecificationComponent component in Enumerables.Create(leftComponent, rightComponent).OfType<SpecificationComponent>())
					component.Point += DragVector * SlowDownFactor;

				OnSpecificationChanged();
			}
		}
		public override void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (IsRightMouseDown) 
			{
				double closestPosition = 
				(
					from position in Scalars.GetIntermediateValuesSymmetric(leftComponent.Position, rightComponent.Position, SegmentSegmentCount + 1)
					let distance = (Curve.GetPoint(position) - mousePosition).Length
					orderby distance ascending
					select position
				)
				.First();

				OnAddSpecification(closestPosition);
			}

			base.MouseUp(mousePosition, mouseButton);
		}
		
		protected void OnSpecificationChanged()
		{
			if (SpecificationChanged != null) SpecificationChanged();
		}
		protected void OnAddSpecification(double position) 
		{
			if (AddSpecification != null) AddSpecification(position);
		}
	}
}
