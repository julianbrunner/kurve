using System;
using Krach.Basics;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	class CurveVelocitySpecification : CurveSpecification
	{
		readonly double position;
		readonly Vector2Double velocity;
		
		public override double Position { get { return position; } }
		public override Term GetErrorTerm(CurvePoint curvePoint) 
		{
			return Term.Sum
			(
				Term.Difference(curvePoint.Derivative.InstantiatedParametricCurve.X, Term.Constant(velocity.X)).Square(),
				Term.Difference(curvePoint.Derivative.InstantiatedParametricCurve.Y, Term.Constant(velocity.Y)).Square()				
			);
		}
		
		public CurveVelocitySpecification(double position, Vector2Double velocity)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.velocity = velocity;
		}
	}
}
