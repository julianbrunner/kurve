using System;
using Krach.Basics;
using Krach.Calculus.Terms;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Calculus.Terms.Composite;
using Krach.Calculus;

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
			IEnumerable<double> result = function.Evaluate(Enumerables.Create(position));

			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
		}
		public Curve TransformPosition(FunctionTerm transformation)
		{
			Variable position = new Variable(1, "t");

			return new Curve(function.Apply(transformation.Apply(position)).Abstract(position));
		}
		public ValueTerm InstantiatePosition(ValueTerm position)
		{
			return function.Apply(position);
		}

		// TODO: remove debug code
		public Curve Scale(double factor)
		{
			Variable position = new Variable(1, "t");

			return new Curve(Term.Scaling(Term.Constant(factor), function.Apply(position)).Abstract(position));
		}
	}
}

