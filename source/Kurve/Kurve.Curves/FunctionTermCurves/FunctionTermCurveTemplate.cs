using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Wrappers.Casadi;
using System.Xml.Linq;

namespace Kurve.Curves
{
	public abstract class FunctionTermCurveTemplate : IEquatable<FunctionTermCurveTemplate>
	{
		readonly FunctionTerm function;

		public int ParameterDimension { get { return function.DomainDimension - 1; } }
		public abstract XElement XElement { get; }

		protected FunctionTermCurveTemplate(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("position");
			if (function.DomainDimension < 1) throw new ArgumentException("parameter 'function' has wrong dimension.");

			this.function = function;
		}

		public override string ToString()
		{
			return function.ToString();
		}
		public override bool Equals(object obj)
		{
			throw new InvalidOperationException();
		}
		public override int GetHashCode()
		{
			throw new InvalidOperationException();
		}
		public bool Equals(FunctionTermCurveTemplate other)
		{
			return object.Equals(this, other);
		}

		public FunctionTermCurve InstantiateParameter(ValueTerm parameter)
		{
			ValueTerm position = Terms.Variable("t");

			return new FunctionTermCurve(function.Apply(position, parameter).Abstract(position));
		}

		public static FunctionTermCurveTemplate Parse(XElement element)
		{
			if (element.Name == PolynomialFunctionTermCurveTemplate.XElementName) return new PolynomialFunctionTermCurveTemplate(element);

			throw new ArgumentException("Parameter 'element' is not a CurveTemplate.");
		}

		public static bool operator ==(FunctionTermCurveTemplate curveTemplate1, FunctionTermCurveTemplate curveTemplate2)
		{
			return object.Equals(curveTemplate1, curveTemplate2);
		}
		public static bool operator !=(FunctionTermCurveTemplate curveTemplate1, FunctionTermCurveTemplate curveTemplate2)
		{
			return !object.Equals(curveTemplate1, curveTemplate2);
		}
	}
}

