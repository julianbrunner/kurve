using System;
using Krach.Basics;
using Krach.Calculus.Terms;
using Krach.Calculus;

namespace Kurve.Curves
{
	public class AccelerationCurveSpecification : PositionedCurveSpecification
	{
		readonly double position;
		readonly Vector2Double acceleration;

		public override double Position { get { return position; } }
		public Vector2Double Acceleration { get { return acceleration; } }
		
		public AccelerationCurveSpecification(double position, Vector2Double acceleration)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.acceleration = acceleration;
		}

		public override ValueTerm GetErrorTerm(Curve curve)
		{
			return Term.Difference(curve.Derivative.Derivative.InstantiatePosition(Term.Constant(position)), Term.Constant(acceleration.X, acceleration.Y));
		}
	}
}
