using System;
using Krach.Basics;
using Krach.Calculus.Terms;
using Krach.Calculus;

namespace Kurve.Curves
{
	public class VelocityCurveSpecification : PositionedCurveSpecification
	{
		readonly double position;
		readonly Vector2Double velocity;

		public override double Position { get { return position; } }
		public Vector2Double Velocity { get { return velocity; } }
		
		public VelocityCurveSpecification(double position, Vector2Double velocity)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.velocity = velocity;
		}

		public override ValueTerm GetErrorTerm(Curve curve)
		{
			return Term.Square(Term.Norm(Term.Difference(curve.Derivative.InstantiatePosition(Term.Constant(position)), Term.Constant(velocity.X, velocity.Y))));
		}
	}
}
