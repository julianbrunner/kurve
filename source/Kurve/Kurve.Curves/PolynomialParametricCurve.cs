using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class PolynomialParametricCurve : ParametricCurve
	{
		PolynomialFunction polynomialFunction;

		public override ParametricCurve Derivative { get { return new PolynomialParametricCurve(polynomialFunction.Derivative); } }

		public PolynomialParametricCurve(PolynomialFunction polynomialFunction)
		{
			if (polynomialFunction == null) throw new ArgumentNullException("polynomialFunction");

			this.polynomialFunction = polynomialFunction;
		}

		public override Vector2Double EvaluatePoint(double position)
		{
			return polynomialFunction.Evaluate(position);
		}
		public override double EvaluateTangentDirection(double position)
		{
			Vector2Double velocity = polynomialFunction.Derivative.Evaluate(position);

			return velocity.Direction;
		}
		public override double EvaluateCurvatureLength(double position)
		{
			Vector2Double velocity = polynomialFunction.Derivative.Evaluate(position);
			Vector2Double acceleration = polynomialFunction.Derivative.Derivative.Evaluate(position);
			// TODO: maybe velocity.LengthSquared is the same as normal.Length
			Vector2Double normal = acceleration - acceleration.Project(velocity);
			Vector2Double curvature = (1 / velocity.LengthSquared) * acceleration.Project(normal);

			return curvature.Length;
		}
	}
}
