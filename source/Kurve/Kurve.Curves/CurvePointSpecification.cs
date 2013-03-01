using System;
using Krach.Basics;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	class CurvePointSpecification : CurveSpecification
	{
		readonly double position;
		readonly Vector2Double point;
		
		public override double Position { get { return position; } }
		public override Term GetErrorTerm(CurvePoint curvePoint) 
		{
			return Term.Sum
			(
				Term.Difference(curvePoint.InstantiatedParametricCurve.X, Term.Constant(point.X)).Square(),
				Term.Difference(curvePoint.InstantiatedParametricCurve.Y, Term.Constant(point.Y)).Square()				
			);
		}
		
		public CurvePointSpecification(double position, Vector2Double point)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.point = point;
		}
	}
}
