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

		public Curve Derivative { get { return new Curve(function.GetDerivatives().Single()); } }

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
		
		public Vector2Double EvaluatePoint(double position)
		{
			IEnumerable<double> result = function.Apply(Terms.Constant(position)).Evaluate();

			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
		}
		public Curve TransformPosition(FunctionTerm transformation)
		{
			ValueTerm position = Terms.Variable("t");

			return new Curve(function.Apply(transformation.Apply(position)).Abstract(position));
		}
		public ValueTerm InstantiatePosition(ValueTerm position)
		{
			return function.Apply(position);
		}

		// TODO: remove debug code
		public Curve Scale(double factor)
		{
			ValueTerm position = Terms.Variable("t");

			return new Curve(Terms.Scaling(Terms.Constant(factor), function.Apply(position)).Abstract(position));
		}
	}
}

