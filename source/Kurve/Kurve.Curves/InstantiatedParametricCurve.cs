using System;
using Krach.Basics;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	public class InstantiatedParametricCurve : ParametricCurve
	{
		public InstantiatedParametricCurve Derivative { get { return new InstantiatedParametricCurve(X.GetDerivative(Position), Y.GetDerivative(Position)); } }

		public InstantiatedParametricCurve(Term x, Term y) : base(x, y) { }

		public Vector2Double EvaluatePoint(double position)
		{
			return new Vector2Double
			(
				X.Substitute(Position, new Constant(position)).Evaluate(),
				Y.Substitute(Position, new Constant(position)).Evaluate()
			);
		}
		public double EvaluateTangentDirection(double position)
		{
			Vector2Double velocity = Derivative.EvaluatePoint(position);

			return velocity.Direction;
		}
		public double EvaluateCurvatureLength(double position)
		{
			Vector2Double velocity = Derivative.EvaluatePoint(position);
			Vector2Double acceleration = Derivative.Derivative.EvaluatePoint(position);
			// TODO: maybe velocity.LengthSquared is the same as normal.Length
			Vector2Double normal = acceleration - acceleration.Project(velocity);
			Vector2Double curvature = (1 / velocity.LengthSquared) * acceleration.Project(normal);

			return curvature.Length;
		}
	}
}

