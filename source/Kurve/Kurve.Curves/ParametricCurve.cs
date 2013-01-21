using System;
using Krach.Basics;
using Krach.Calculus.Terms;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class ParametricCurve
	{
		readonly Term x;
		readonly Term y;
		readonly Variable position;
		readonly IEnumerable<Variable> parameters;

		public Term X { get { return x; } }
		public Term Y { get { return y; } }
		public Variable Position { get { return position; } }
		public IEnumerable<Variable> Parameters { get { return parameters; } }
		public ParametricCurve Derivative { get { return new ParametricCurve(X.GetDerivative(position), Y.GetDerivative(position), position, parameters); } }

		public ParametricCurve(Term x, Term y, Variable position, IEnumerable<Variable> parameters)
		{
			if (x == null) throw new ArgumentNullException("x");
			if (y == null) throw new ArgumentNullException("y");
			if (position == null) throw new ArgumentNullException("position");
			if (parameters == null) throw new ArgumentNullException("parameters");

			this.x = x;
			this.y = y;
			this.position = position;
			this.parameters = parameters;
		}
		
		public override string ToString()
		{
			return string.Format("{0}\n{1}", x, y);
		}
		
		public Vector2Double EvaluatePoint(double positionValue)
		{
			ParametricCurve point = InstantiatePosition(new Constant(positionValue));

			return new Vector2Double(point.x.Evaluate(), point.y.Evaluate());
		}
		public double EvaluateTangentDirection(double positionValue)
		{
			Vector2Double velocity = Derivative.EvaluatePoint(positionValue);

			return velocity.Direction;
		}
		public double EvaluateCurvatureLength(double positionValue)
		{
			Vector2Double velocity = Derivative.EvaluatePoint(positionValue);
			Vector2Double acceleration = Derivative.Derivative.EvaluatePoint(positionValue);
			// TODO: maybe velocity.LengthSquared is the same as normal.Length
			Vector2Double normal = acceleration - acceleration.Project(velocity);
			Vector2Double curvature = (1 / velocity.LengthSquared) * acceleration.Project(normal);

			return curvature.Length;
		}
		public ParametricCurve InstantiatePosition(Term term)
		{
			return new ParametricCurve(x.Substitute(position, term), y.Substitute(position, term), position, parameters);
		}
		public ParametricCurve RenamePosition(Variable newPosition)
		{
			return new ParametricCurve(x.Substitute(position, newPosition), y.Substitute(position, newPosition), newPosition, parameters);
		}
		public ParametricCurve InstantiateParameters(IEnumerable<Term> terms)
		{
			terms = terms.ToArray();
			Term x = Enumerable.Zip(parameters, terms, Tuple.Create).Aggregate(X, (term, item) => term.Substitute(item.Item1, item.Item2));
			Term y = Enumerable.Zip(parameters, terms, Tuple.Create).Aggregate(Y, (term, item) => term.Substitute(item.Item1, item.Item2));

			return new ParametricCurve(x, y, position, parameters);
		}
		public ParametricCurve RenameParameters(IEnumerable<Variable> newParameters) 
		{
			newParameters = newParameters.ToArray();

			Term x = Enumerable.Zip(parameters, newParameters, Tuple.Create).Aggregate(X, (term, item) => term.Substitute(item.Item1, item.Item2));
			Term y = Enumerable.Zip(parameters, newParameters, Tuple.Create).Aggregate(Y, (term, item) => term.Substitute(item.Item1, item.Item2));

			return new ParametricCurve(x, y, position, newParameters);
		}

		public static ParametricCurve CreatePolynomialParametricCurveTemplate(int degree)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			Variable position = new Variable("position");
			IEnumerable<Variable> coefficients = 
				from index in Enumerable.Range(0, degree)
				from component in Enumerables.Create("x", "y")
				select new Variable(string.Format("coefficient_{0}_{1}", index, component));

			Term x = Term.Sum
			(
				from index in Enumerable.Range(0, degree)
				let coefficient = coefficients.ElementAt(index * 2 + 0)
				let power = position.Exponentiate(new Constant(index))
				select Term.Product(coefficient, power)
			);
			Term y = Term.Sum
			(
				from index in Enumerable.Range(0, degree)
				let coefficient = coefficients.ElementAt(index * 2 + 1)
				let power = position.Exponentiate(new Constant(index))
				select Term.Product(coefficient, power)
			);

			return new ParametricCurve(x, y, position, coefficients);
		}
	}
}

