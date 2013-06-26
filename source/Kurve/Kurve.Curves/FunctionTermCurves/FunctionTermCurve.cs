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
		readonly FunctionTerm point;
		readonly FunctionTerm velocity;
		readonly FunctionTerm acceleration;

		public FunctionTerm Point { get { return point; } }
		public FunctionTerm Velocity { get { return velocity; } }
		public FunctionTerm Acceleration { get { return acceleration; } }

		public FunctionTermCurve(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("function");
			if (function.DomainDimension != 1) throw new ArgumentException("parameter 'function' has wrong dimension.");

			this.point = function;
			this.velocity = point.GetDerivatives().Single();
			this.acceleration = velocity.GetDerivatives().Single();
		}

		public override Vector2Double GetPoint(double position)
		{
			return Evaluate(point, position);
		}
		public override Vector2Double GetVelocity(double position)
		{
			return Evaluate(velocity, position);
		}
		public override Vector2Double GetAcceleration(double position)
		{
			return Evaluate(acceleration, position);
		}

		public FunctionTermCurve TransformPosition(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new FunctionTermCurve(point.Apply(transformation.Apply(position)).Abstract(position));
		}
		public FunctionTermCurve TransformPoint(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new FunctionTermCurve(transformation.Apply(point.Apply(position)).Abstract(position));
		}

		static Vector2Double Evaluate(FunctionTerm function, double value)
		{
			IEnumerable<double> result = function.Apply(Terms.Constant(value)).Evaluate();

			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
		}
	}
}

