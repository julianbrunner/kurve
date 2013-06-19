using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	public class FunctionTermCurve : Curve
	{
		readonly FunctionTerm function;

		public FunctionTerm Point { get { return function; } }
		public FunctionTerm Velocity { get { return Point.GetDerivatives().Single(); } }
		public FunctionTerm Acceleration { get { return Velocity.GetDerivatives().Single(); } }

		public FunctionTermCurve(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("position");
			if (function.DomainDimension != 1) throw new ArgumentException("parameter 'function' has wrong dimension.");

			this.function = function;
		}

		public override Vector2Double GetPoint(double position)
		{
			return Evaluate(Point, position);
		}
		public override Vector2Double GetVelocity(double position)
		{
			return Evaluate(Velocity, position);
		}
		public override Vector2Double GetAcceleration(double position)
		{
			return Evaluate(Acceleration, position);
		}

		public FunctionTermCurve TransformPosition(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new FunctionTermCurve(function.Apply(transformation.Apply(position)).Abstract(position));
		}
		public FunctionTermCurve TransformPoint(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new FunctionTermCurve(transformation.Apply(function.Apply(position)).Abstract(position));
		}

		static Vector2Double Evaluate(FunctionTerm function, double value)
		{
			IEnumerable<double> result = function.Apply(Terms.Constant(value)).Evaluate();

			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
		}
	}
}

