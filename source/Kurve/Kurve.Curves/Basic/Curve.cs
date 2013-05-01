using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	public class Curve
	{
		readonly FunctionTerm function;

		public FunctionTerm Point { get { return function; } }
		public FunctionTerm Velocity { get { return Point.GetDerivatives().Single(); } }
		public FunctionTerm Acceleration { get { return Velocity.GetDerivatives().Single(); } }
//		public FunctionTerm Speed
//		{
//			get
//			{
//				ValueTerm position = Terms.Variable("t");
//
//				ValueTerm velocity = Velocity.Apply(position);
//
//				return Terms.Norm(velocity).Abstract(position);
//			}
//		}
//		public FunctionTerm Direction
//		{
//			get
//			{
//				ValueTerm position = Terms.Variable("t");
//
//				ValueTerm velocity = Velocity.Apply(position);
//
//				return Terms.Scaling(Terms.Invert(Terms.Norm(velocity)), velocity).Abstract(position);
//			}
//		}
//		public FunctionTerm Curvature
//		{
//			get
//			{
//				ValueTerm position = Terms.Variable("t");
//
//				ValueTerm acceleration = Acceleration.Apply(position);
//				
//				ValueTerm speed = Speed.Apply(position);
//				ValueTerm linearDirection = Direction.Apply(position);
//				ValueTerm angularDirection = Terms.Vector(linearDirection.Select(1), Terms.Negate(linearDirection.Select(0)));
//				ValueTerm angularAcceleration = Terms.DotProduct(acceleration, angularDirection);
//
//				return Terms.Product(Terms.Invert(Terms.Square(speed)), angularAcceleration).Abstract(position);
//			}
//		}

		public Curve(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("position");
			if (function.DomainDimension != 1) throw new ArgumentException("parameter 'function' has wrong dimension.");

			this.function = function;
		}
		
		public override string ToString()
		{
			return function.ToString();
		}

		public Curve TransformPosition(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new Curve(function.Apply(transformation.Apply(position)).Abstract(position));
		}
		public Curve TransformPoint(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new Curve(transformation.Apply(function.Apply(position)).Abstract(position));
		}
	}
}

