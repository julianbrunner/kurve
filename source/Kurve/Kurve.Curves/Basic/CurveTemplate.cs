using System;
using Krach.Basics;
using Krach.Calculus.Terms;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Calculus.Terms.Composite;
using Krach.Calculus;
using Krach.Calculus.Terms.Basic.Definitions;
using Krach.Calculus.Terms.Notation;
using Krach.Calculus.Terms.Notation.Custom;

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
			Variable position = new Variable(1, "t");

			return new Curve(function.Apply(position, parameter).Abstract(position));
		}

		public static CurveTemplate CreatePolynomial(int degree)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			return new CurveTemplate
			(
				new FunctionDefinition
				(
					string.Format("polynomial_curve_template_{0}_{1}", 2, degree),
					Rewriting.CompleteNormalization.Rewrite(Term.Polynomial(2, degree)),
					new BasicSyntax("pct")
				)
			);
		}
	}
}

