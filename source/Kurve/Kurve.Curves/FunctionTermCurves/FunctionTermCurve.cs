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
		readonly FunctionTerm speed;
		readonly FunctionTerm direction;
		readonly FunctionTerm curvature;

		public FunctionTerm Point { get { return point; } }
		public FunctionTerm Velocity { get { return velocity; } }
		public FunctionTerm Acceleration { get { return acceleration; } }
		public FunctionTerm Speed { get { return speed; } }
		public FunctionTerm Direction { get { return direction; } }
		public FunctionTerm Curvature { get { return curvature; } }

		public FunctionTermCurve(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("function");
			if (function.DomainDimension != 1) throw new ArgumentException("parameter 'function' has wrong dimension.");

			ValueTerm position = Terms.Variable("t");

			ValueTerm point = function.Apply(position);
			ValueTerm velocity = function.GetDerivatives().Single().Apply(position);
			ValueTerm acceleration = function.GetDerivatives().Single().GetDerivatives().Single().Apply(position);
			ValueTerm speed = Terms.Norm(velocity);
			ValueTerm direction = Terms.Scaling(Terms.Invert(speed), velocity);

			// TODO: maybe we can get rid of some normalizations here (seems like angularDirection uses the normalized direction just to divide it by the square of the speed later)
			ValueTerm angularDirection = Terms.Vector(direction.Select(1), Terms.Negate(direction.Select(0)));
			ValueTerm angularAcceleration = Terms.DotProduct(acceleration, angularDirection);
			ValueTerm curvature = Terms.Scaling(Terms.Invert(Terms.Square(speed)), angularAcceleration);

			this.point = point.Abstract(position);
			this.velocity = velocity.Abstract(position);
			this.acceleration = acceleration.Abstract(position);
			this.speed = speed.Abstract(position);
			this.direction = direction.Abstract(position);
			this.curvature = curvature.Abstract(position);
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

		public FunctionTermCurve TransformInput(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new FunctionTermCurve(point.Apply(transformation.Apply(position)).Abstract(position));
		}
		public FunctionTermCurve TransformOutput(FunctionTerm transformation)
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

