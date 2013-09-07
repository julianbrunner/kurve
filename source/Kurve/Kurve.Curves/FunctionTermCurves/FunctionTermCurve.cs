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
		readonly FunctionTerm jerk;
		readonly FunctionTerm speed;
		readonly FunctionTerm direction;
		readonly FunctionTerm curvature;

		public FunctionTerm Point { get { return point; } }
		public FunctionTerm Velocity { get { return velocity; } }
		public FunctionTerm Acceleration { get { return acceleration; } }
		public FunctionTerm Jerk { get { return jerk; } }
		public FunctionTerm Speed { get { return speed; } }
		public FunctionTerm Direction { get { return direction; } }
		public FunctionTerm Curvature { get { return curvature; } }

		public FunctionTermCurve(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("function");
			if (function.DomainDimension != 1) throw new ArgumentException("parameter 'function' has wrong domain dimension.");
			if (function.CodomainDimension != 2) throw new ArgumentException("parameter 'function' has wrong codomain dimension.");

			this.point = function;
			this.velocity = point.GetDerivatives().Single();
			this.acceleration = velocity.GetDerivatives().Single();
			this.jerk = acceleration.GetDerivatives().Single();
			
			ValueTerm position = Terms.Variable("t");

			ValueTerm speedValue = Terms.Norm(velocity.Apply(position));
			this.speed = speedValue.Abstract(position);

			ValueTerm directionValue = Terms.Angle(velocity.Apply(position));
			this.direction = directionValue.Abstract(position);

			ValueTerm curvatureValue = Terms.Quotient(direction.GetDerivatives().Single().Apply(position), speedValue);
			this.curvature = curvatureValue.Abstract(position);
		}

		public override Vector2Double GetPoint(double position)
		{
			return EvaluateVector(point, position);
		}
		public override double GetSpeed(double position)
		{
			return EvaluateScalar(speed, position);
		}
		public override double GetDirection(double position)
		{
			return EvaluateScalar(direction, position);
		}
		public override double GetCurvature(double position)
		{
			return EvaluateScalar(curvature, position);
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

		static double EvaluateScalar(FunctionTerm function, double value)
		{
			IEnumerable<double> result = function.Apply(Terms.Constant(value)).Evaluate();

			return result.Single();
		}
		static Vector2Double EvaluateVector(FunctionTerm function, double value)
		{
			IEnumerable<double> result = function.Apply(Terms.Constant(value)).Evaluate();

			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
		}
	}
}

