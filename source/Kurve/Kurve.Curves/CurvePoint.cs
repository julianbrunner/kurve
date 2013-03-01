using System;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	class CurvePoint
	{
		readonly ParametricCurve parametricCurve;
		readonly double position;
		
		public ParametricCurve ParametricCurve { get { return parametricCurve; } }
		public double Position { get { return position; } }
		public CurvePoint Derivative { get { return new CurvePoint(parametricCurve.Derivative, position); } }
		public ParametricCurve InstantiatedParametricCurve { get { return parametricCurve.InstantiatePosition(Term.Constant(position)); } }
		
		public CurvePoint(ParametricCurve parametricCurve, double position)
		{
			if (parametricCurve == null) throw new ArgumentNullException("parametricCurve");
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");

			this.parametricCurve = parametricCurve;
			this.position = position;
		}
	}
}
