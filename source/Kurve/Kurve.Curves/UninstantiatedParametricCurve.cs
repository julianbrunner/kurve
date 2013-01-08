using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Calculus.Terms;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class UninstantiatedParametricCurve : ParametricCurve
	{
		readonly IEnumerable<Variable> coefficients;
		
		public IEnumerable<Variable> Coefficients { get { return coefficients; } }

		public UninstantiatedParametricCurve(IEnumerable<Variable> coefficients, Term x, Term y) : base(x, y)
		{
			if (coefficients == null) throw new ArgumentNullException("coefficients");

			this.coefficients = coefficients;
		}

		public static UninstantiatedParametricCurve CreatePolynomialParametricCurveClass(int degree)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");
			
			IEnumerable<Variable> coefficients = 
				from index in Enumerable.Range(0, degree)
				from component in Enumerables.Create("x", "y")
				select new Variable(string.Format("coefficient_{0}_{1}", index, component));
			Term x = Term.Sum
			(
				from index in Enumerable.Range(0, degree)
				let coefficient = coefficients.ElementAt(index * 2 + 0)
				let power = Position.Exponentiate(Term.Constant(index))
				select Term.Product(coefficient, power)
			);
			Term y = Term.Sum
			(
				from index in Enumerable.Range(0, degree)
				let coefficient = coefficients.ElementAt(index * 2 + 1)
				let power = Position.Exponentiate(Term.Constant(index))
				select Term.Product(coefficient, power)
			);

			return new UninstantiatedParametricCurve(coefficients, x, y);
		}
		
		public UninstantiatedParametricCurve ReplaceCoefficients(IEnumerable<Variable> newCoefficients)
		{
			Term x = Enumerable.Zip(coefficients, newCoefficients, Tuple.Create).Aggregate(X, (term, item) => term.Substitute(item.Item1, item.Item2));
			Term y = Enumerable.Zip(coefficients, newCoefficients, Tuple.Create).Aggregate(Y, (term, item) => term.Substitute(item.Item1, item.Item2));

			return new UninstantiatedParametricCurve(newCoefficients, x, y);
		}
		public InstantiatedParametricCurve Instantiate(IEnumerable<double> values)
		{
			Term x = Enumerable.Zip(coefficients, values, Tuple.Create).Aggregate(X, (term, item) => term.Substitute(item.Item1, new Constant(item.Item2)));
			Term y = Enumerable.Zip(coefficients, values, Tuple.Create).Aggregate(Y, (term, item) => term.Substitute(item.Item1, new Constant(item.Item2)));

			return new InstantiatedParametricCurve(x, y);
		}
	}
}

