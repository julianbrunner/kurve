using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	public class CurveTemplate
	{
		readonly FunctionTerm function;

		public int ParameterDimension { get { return function.DomainDimension - 1; } }

		CurveTemplate(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("position");
			if (function.DomainDimension < 1) throw new ArgumentException("parameter 'function' has wrong dimension.");

			this.function = function;
		}

		public override string ToString()
		{
			return function.ToString();
		}

		public Curve InstantiateParameter(ValueTerm parameter)
		{
			ValueTerm position = Terms.Variable("t");

			return new Curve(function.Apply(position, parameter).Abstract(position));
		}

		public static CurveTemplate CreatePolynomial(int degree)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			return new CurveTemplate(Terms.Polynomial(2, degree));
		}
	}
}

