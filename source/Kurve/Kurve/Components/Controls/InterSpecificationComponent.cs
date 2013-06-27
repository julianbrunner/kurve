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

namespace Kurve.Component
{
	class InterSpecificationComponent : PositionedControlComponent
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);

		readonly SpecificationComponent leftComponent;
		readonly SpecificationComponent rightComponent;

		Orthotope2Double Bounds { get { return new Orthotope2Double(Point - 0.5 * size, Point + 0.5 * size); } }

		public override double Position { get { return (leftComponent.Position + rightComponent.Position) / 2; } }
		public Vector2Double Point { get { return Curve == null ? Vector2Double.Origin : Curve.GetPoint(Position); } }

		public InterSpecificationComponent(Component parent, CurveComponent curveComponent, SpecificationComponent leftComponent, SpecificationComponent rightComponent) : base(parent, curveComponent)
		{
			if (leftComponent == null) throw new ArgumentNullException("leftComponent");
			if (rightComponent == null) throw new ArgumentNullException("rightComponent");
		
			this.leftComponent = leftComponent;
			this.rightComponent = rightComponent;
		}

		public override void Draw(Context context)
		{
			if (Curve == null) return;
			if (!Selected) return;

			foreach (Tuple<double, double> positions in Scalars.GetIntermediateValues(leftComponent.Position, rightComponent.Position, 100).GetRanges()) 
			{
				double stretchFactor = Curve.GetVelocity((positions.Item1 + positions.Item2) / 2).Length / BasicSpecification.CurveLength;

				OrderedRange<double> source = new OrderedRange<double>(0.75, 1.0);
				OrderedRange<double> destination = new OrderedRange<double>(0.0, 1.0);

				IMap<double, double> amplifier = new RangeMap(source, destination, Mappers.Linear);

				Krach.Graphics.Color color = Colors.Green;
				if (stretchFactor < 1) color = Krach.Graphics.Color.InterpolateHsv(Colors.Blue, Colors.Green, Scalars.InterpolateLinear, amplifier.Map((1.0 * stretchFactor).Clamp(source)));
				if (stretchFactor > 1) color = Krach.Graphics.Color.InterpolateHsv(Colors.Red, Colors.Green, Scalars.InterpolateLinear, amplifier.Map((1.0 / stretchFactor).Clamp(source)));

				color = Krach.Graphics.Color.FromRgba(color.Red, color.Green, color.Blue, 0.3);

				InterfaceUtility.DrawLine(context, Curve.GetPoint(positions.Item1), Curve.GetPoint(positions.Item2), 8, color);
			}

			base.Draw(context);
		}

		public override bool Contains(Vector2Double position)
		{
			if ((leftComponent.CurrentPoint - position).Length < 15) return false;
			if ((rightComponent.CurrentPoint - position).Length < 15) return false;

			foreach (double testedPosition in Scalars.GetIntermediateValues(leftComponent.Position, rightComponent.Position, 100)) 
				if ((Curve.GetPoint(testedPosition) - position).Length < 10) return true; 

			return false;
		}
	}
}
