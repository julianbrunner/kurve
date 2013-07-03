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
			if (Curve == null) return;

			foreach (Tuple<double, double> positions in Scalars.GetIntermediateValuesSymmetric(leftComponent.Position, rightComponent.Position, 100).GetRanges()) 
			{
				double stretchFactor = Curve.GetVelocity((positions.Item1 + positions.Item2) / 2).Length / BasicSpecification.CurveLength;

				if (Selected) {
					Krach.Graphics.Color selectionColor = Colors.Green.ReplaceAlpha(0.3);
					Drawing.DrawLine(context, Curve.GetPoint(positions.Item1), Curve.GetPoint(positions.Item2), 8, selectionColor);
				}

				Krach.Graphics.Color color = StretchedColor(Krach.Graphics.Colors.Black, stretchFactor);
				Drawing.DrawLine(context, Curve.GetPoint(positions.Item1), Curve.GetPoint(positions.Item2), 2, color);
			}

			base.Draw(context);
		}
		static Krach.Graphics.Color StretchedColor(Krach.Graphics.Color baseColor, double stretchFactor)
		{
			OrderedRange<double> source = new OrderedRange<double>(0.75, 1.0);
			OrderedRange<double> destination = new OrderedRange<double>(0.0, 1.0);
		
			IMap<double, double> amplifier = new RangeMap(source, destination, Mappers.Linear);
		
			if (stretchFactor < 1) return Krach.Graphics.Color.InterpolateHsv(Colors.Blue, baseColor, Scalars.InterpolateLinear, amplifier.Map((1.0 * stretchFactor).Clamp(source)));
			if (stretchFactor > 1) return Krach.Graphics.Color.InterpolateHsv(Colors.Red, baseColor, Scalars.InterpolateLinear, amplifier.Map((1.0 / stretchFactor).Clamp(source)));
			return baseColor;
		}

		public override bool Contains(Vector2Double position)
		{
			if (leftComponent is SpecificationComponent && (Curve.GetPoint(leftComponent.Position) - position).Length < 15) return false;
			if (rightComponent is SpecificationComponent && (Curve.GetPoint(rightComponent.Position) - position).Length < 15) return false;

			Console.WriteLine ("not close to either");

			foreach (double testedPosition in Scalars.GetIntermediateValuesSymmetric(leftComponent.Position, rightComponent.Position, 100)) {
				if ((Curve.GetPoint(testedPosition) - position).Length < 10) return true; 
			}

			return false;
		}

		public override void MouseMove(Vector2Double mousePosition)
		{
			base.MouseMove(mousePosition);

			if (Dragging) 
			{
				foreach (PointSpecificationComponent component in Enumerables.Create(leftComponent, rightComponent).OfType<PointSpecificationComponent>())
					component.Point += DragVector;

				OnSpecificationChanged();
			}
		}

		public override void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (IsRightMouseDown) 
			{
				double closestPosition = 
				(
					from position in Scalars.GetIntermediateValuesSymmetric(leftComponent.Position, rightComponent.Position, 100)
					let distance = (Curve.GetPoint(position) - mousePosition).Length
					orderby distance ascending
					select position
				)
				.First();

				OnAddSpecification(closestPosition);
			}

			base.MouseUp(mousePosition,mouseButton);
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
