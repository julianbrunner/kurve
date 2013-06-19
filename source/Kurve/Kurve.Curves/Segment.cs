using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	class Segment
	{
		readonly FunctionTermCurve curve;
		readonly FunctionTerm positionTransformation;

		public FunctionTermCurve LocalCurve { get { return curve; } }
		public FunctionTermCurve GlobalCurve { get { return curve.TransformPosition(positionTransformation); } }
		public FunctionTerm PositionTransformation { get { return positionTransformation; } }

		public Segment(FunctionTermCurve curve, FunctionTerm positionTransformation)
		{
			if (curve == null) throw new ArgumentNullException("curve");
			if (positionTransformation == null) throw new ArgumentNullException("positionTransformation");

			this.curve = curve;
			this.positionTransformation = positionTransformation;
		}

		public bool Contains(double position)
		{
			double localPosition = positionTransformation.Apply(Terms.Constant(position)).Evaluate().Single();

			return new OrderedRange<double>(0, 1).Contains(localPosition);
		}
	}
}

