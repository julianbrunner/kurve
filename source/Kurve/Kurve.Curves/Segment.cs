using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	public class Segment
	{
		readonly CurveTemplate curveTemplate;
		readonly ValueTerm parameter;
		readonly FunctionTerm positionTransformation;

		public CurveTemplate CurveTemplate { get { return curveTemplate; } }
		public ValueTerm Parameter { get { return parameter; } }
		public FunctionTerm PositionTransformation { get { return positionTransformation; } }

		public Segment(CurveTemplate curveTemplate, FunctionTerm positionTransformation, int index)
		{
			if (curveTemplate == null) throw new ArgumentNullException("curveTemplate");
			if (positionTransformation == null) throw new ArgumentNullException("positionTransformation");
			if (index < 0) throw new ArgumentOutOfRangeException("index");

			this.curveTemplate = curveTemplate;
			this.parameter = Terms.Variable(string.Format("sp_{0}", index), curveTemplate.ParameterDimension);
			this.positionTransformation = positionTransformation;
		}

		public Curve GetLocalCurve()
		{
			return curveTemplate.InstantiateParameter(parameter);
		}
		public Curve GetGlobalCurve()
		{
			return curveTemplate.InstantiateParameter(parameter).TransformPosition(positionTransformation);
		}
		public Curve InstantiateLocalCurve(ValueTerm value)
		{
			return curveTemplate.InstantiateParameter(value);
		}
	}
}

