using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;

namespace Kurve.Curves
{
	public class PolynomialFunction
	{
		IEnumerable<Vector2Double> coefficients;

		public PolynomialFunction Derivative
		{
			get
			{
				return new PolynomialFunction(Enumerable.Range(1, coefficients.Count() - 1).Select(index => index * coefficients.ElementAt(index)));
			}
		}

		public PolynomialFunction(IEnumerable<Vector2Double> coefficients)
		{
			if (coefficients == null) throw new ArgumentNullException("coefficients");

			this.coefficients = coefficients;
		}

		public Vector2Double Evaluate(double position)
		{
			Vector2Double result = Vector2Double.Origin;

			foreach (Vector2Double coefficient in coefficients.Reverse()) result = result * position + coefficient;

			return result;
		}
	}
}