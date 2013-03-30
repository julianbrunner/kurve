using System;
using System.Collections.Generic;
using Krach.Calculus.Terms;
using Krach.Calculus.Terms.Composite;
using Krach.Calculus;

namespace Kurve.Curves.Segmentation
{
	class Segment
	{
		readonly Curve curve;
		readonly Variable parameter;
		readonly FunctionTerm positionTransformation;

		public Curve Curve { get { return curve; } }
		public Variable Parameter { get { return parameter; } }
		public FunctionTerm PositionTransformation { get { return positionTransformation; } }

		public Segment(Curve curve, Variable parameter, FunctionTerm positionTransformation)
		{
			if (curve == null) throw new ArgumentNullException("curve");
			if (parameter == null) throw new ArgumentNullException("parameter");
			if (positionTransformation == null) throw new ArgumentNullException("positionTransformation");

			this.curve = curve;
			this.parameter = parameter;
			this.positionTransformation = positionTransformation;
		}

		public Curve GetLocalCurve()
		{
			Variable position = new Variable(1, "t");

			return new Curve(curve.Function.Apply(position, parameter).Abstract(position));
		}
		public Curve GetGlobalCurve()
		{
			Variable position = new Variable(1, "t");

			return new Curve(curve.Function.Apply(positionTransformation.Apply(position), parameter).Abstract(position));
		}
	}
}
