using System;
using Krach.Basics;
using Krach.Calculus.Terms;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Calculus.Terms.Composite;
using Krach.Calculus;

namespace Kurve.Curves
{
	// TODO: can we avoid exposing the function property?
	public class Curve
	{
		readonly FunctionTerm function;

		public FunctionTerm Function { get { return function; } }
		public int PositionDimension { get { return 1; } }
		public int ParameterDimension { get { return function.DomainDimension - 1; } }
		public Curve Derivative { get { return new Curve(function.GetDerivatives().First()); } }

		public Curve(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("position");

			this.function = function;
		}
		
		public override string ToString()
		{
			return function.ToString();
		}
		
		public Vector2Double EvaluatePoint(double position)
		{
			if (ParameterDimension != 0) throw new InvalidOperationException("Cannot evaluate uninstantiated curve.");

			IEnumerable<double> result = function.Evaluate(Enumerables.Create(position));

			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
		}
		public Curve Instantiate(ValueTerm parameter)
		{
			Variable position = new Variable(1, "t");

			return new Curve(function.Apply(position, parameter).Abstract(position));
		}

		public static Curve CreatePolynomialCurve(int degree)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			return new Curve(Term.Polynomial(2, degree));
		}
	}
}

