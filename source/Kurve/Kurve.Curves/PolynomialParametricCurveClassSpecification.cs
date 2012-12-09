using System;
using System.Linq;
using Krach.Basics;
using System.Collections.Generic;

namespace Kurve.Curves
{
	public class PolynomialParametricCurveClassSpecification : ParametricCurveClassSpecification
	{
		readonly int order;

		public override int ParameterCount { get { return order * 2; } }

		public PolynomialParametricCurveClassSpecification(int order)
		{
			if (order < 0) throw new ArgumentOutOfRangeException("order");

			this.order = order;
		}

		public override ParametricCurve CreateCurve(IEnumerable<double> parameters)
		{
			if (parameters.Count() != ParameterCount) throw new ArgumentException("Parameter 'parameters' has not the right item count.");

			IEnumerable<Vector2Double> coefficients =
				from index in Enumerable.Range(0, order)
				let x = parameters.ElementAt(index * 2 + 0)
				let y = parameters.ElementAt(index * 2 + 1)
				select new Vector2Double(x, y);

			return new PolynomialParametricCurve(new PolynomialFunction(coefficients));
		}
	}
}

