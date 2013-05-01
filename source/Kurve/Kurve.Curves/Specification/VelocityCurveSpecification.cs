using System;
using Krach.Basics;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	public class VelocityCurveSpecification : PositionedCurveSpecification
	{
		readonly double position;
		readonly Vector2Double velocity;

		public override double Position { get { return position; } }
		public Vector2Double Direction { get { return velocity; } }
		
		public VelocityCurveSpecification(double position, Vector2Double velocity)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.velocity = velocity;
		}

		public override ValueTerm GetErrorTerm(Curve curve)
		{
			return Terms.Difference(curve.Velocity.Apply(Terms.Constant(position)), Terms.Constant(velocity.X, velocity.Y));
		}
	}
}
