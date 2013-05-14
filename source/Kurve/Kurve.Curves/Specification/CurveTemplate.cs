using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Wrappers.Casadi;
using System.Xml.Linq;

namespace Kurve.Curves
{
	public abstract class CurveTemplate
	{
		readonly FunctionTerm function;

		public int ParameterDimension { get { return function.DomainDimension - 1; } }
		public abstract XElement XElement { get; }

		protected CurveTemplate(FunctionTerm function)
		{
			if (function == null) throw new ArgumentNullException("position");
			if (function.DomainDimension < 1) throw new ArgumentException("parameter 'function' has wrong dimension.");

			this.function = function;
		}

		public override string ToString()
		{
			return function.ToString();
		}

		public Curve InstantiateParameter(ValueTerm parameter)
		{
			ValueTerm position = Terms.Variable("t");

			return new Curve(function.Apply(position, parameter).Abstract(position));
		}

		public static CurveTemplate Parse(XElement element)
		{
			switch (element.Name.ToString()) 
			{
				case "polynomial_curve_template": return new PolynomialCurveTemplate(element);
				default: throw new ArgumentException("Parameter 'element' is not a CurveTemplate.");
			}
		}
	}
}

