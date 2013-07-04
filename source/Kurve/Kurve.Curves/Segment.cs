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
		readonly FunctionTermCurve localCurve;
		readonly FunctionTermCurve globalCurve;
		readonly FunctionTerm positionTransformation;

		public FunctionTermCurve LocalCurve { get { return localCurve; } }
		public FunctionTermCurve GlobalCurve { get { return globalCurve; } }
		public FunctionTerm PositionTransformation { get { return positionTransformation; } }

		public Segment(FunctionTermCurve localCurve, FunctionTerm positionTransformation)
		{
			if (localCurve == null) throw new ArgumentNullException("localCurve");
			if (positionTransformation == null) throw new ArgumentNullException("positionTransformation");

			this.localCurve = localCurve;
			this.globalCurve = localCurve.TransformInput(positionTransformation);
			this.positionTransformation = positionTransformation;
		}

		public bool Contains(double position)
		{
			double localPosition = positionTransformation.Apply(Terms.Constant(position)).Evaluate().Single();

			return new OrderedRange<double>(0, 1).Contains(localPosition);
		}
	}
}

