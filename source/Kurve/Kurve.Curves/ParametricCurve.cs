using System;
using Krach.Basics;

namespace Kurve.Curves
{
	public abstract class ParametricCurve
	{
		public abstract Vector2Double EvaluatePoint(double position);
		public abstract double EvaluateTangentDirection(double position);
		public abstract double EvaluateCurvatureLength(double position);
	}
}

