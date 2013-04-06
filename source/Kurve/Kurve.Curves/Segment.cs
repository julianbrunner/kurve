using System;
using System.Linq;
using Kurve.Ipopt;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Krach.Calculus.Terms;
using Krach.Calculus;
using Krach.Calculus.Terms.Composite;
using Krach.Calculus.Terms.Notation;

namespace Kurve.Curves
{
	class Segment
	{
		readonly CurveTemplate curveTemplate;
		readonly Variable parameter;
		readonly FunctionTerm positionTransformation;

		public CurveTemplate CurveTemplate { get { return curveTemplate; } }
		public Variable Parameter { get { return parameter; } }
		public FunctionTerm PositionTransformation { get { return positionTransformation; } }

		public Segment(CurveTemplate curveTemplate, FunctionTerm positionTransformation, int index)
		{
			if (curveTemplate == null) throw new ArgumentNullException("curveTemplate");
			if (positionTransformation == null) throw new ArgumentNullException("positionTransformation");
			if (index < 0) throw new ArgumentOutOfRangeException("index");

			this.curveTemplate = curveTemplate;
			this.parameter = new Variable(curveTemplate.ParameterDimension, string.Format("sp_{0}", index));
			this.positionTransformation = positionTransformation;
		}

		public Curve Instantiate(IEnumerable<double> values)
		{
			return curveTemplate.InstantiateParameter(Term.Constant(values));
		}
		public Curve GetLocalCurve()
		{
			return curveTemplate.InstantiateParameter(parameter);
		}
		public Curve GetGlobalCurve()
		{
			return curveTemplate.InstantiateParameter(parameter).TransformPosition(positionTransformation);
		}
	}
}

