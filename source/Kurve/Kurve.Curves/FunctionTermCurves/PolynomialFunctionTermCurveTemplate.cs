using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Wrappers.Casadi;
using System.Xml.Linq;

namespace Kurve.Curves
{
	public class PolynomialFunctionTermCurveTemplate : FunctionTermCurveTemplate, IEquatable<PolynomialFunctionTermCurveTemplate>
	{
		readonly int degree;

		public static string XElementName { get { return "polynomial_curve_template"; } }

		public override XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("degree", degree)
				);
			}
		}

		public PolynomialFunctionTermCurveTemplate(int degree) : base(Create(degree))
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			this.degree = degree;
		}
		public PolynomialFunctionTermCurveTemplate(XElement source) : this(Create(source)) { }

		public override bool Equals(object obj)
		{
			return obj is PolynomialFunctionTermCurveTemplate && Equals(this, (PolynomialFunctionTermCurveTemplate)obj);
		}
		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode() ^ degree.GetHashCode();
		}
		public bool Equals(PolynomialFunctionTermCurveTemplate other)
		{
			return object.Equals(this, other);
		}

		public static bool operator ==(PolynomialFunctionTermCurveTemplate curveTemplate1, PolynomialFunctionTermCurveTemplate curveTemplate2)
		{
			return object.Equals(curveTemplate1, curveTemplate2);
		}
		public static bool operator !=(PolynomialFunctionTermCurveTemplate curveTemplate1, PolynomialFunctionTermCurveTemplate curveTemplate2)
		{
			return !object.Equals(curveTemplate1, curveTemplate2);
		}
		
		static bool Equals(PolynomialFunctionTermCurveTemplate curveTemplate1, PolynomialFunctionTermCurveTemplate curveTemplate2) 
		{
			if (object.ReferenceEquals(curveTemplate1, curveTemplate2)) return true;
			if (object.ReferenceEquals(curveTemplate1, null) || object.ReferenceEquals(curveTemplate2, null)) return false;
			
			return curveTemplate1.degree == curveTemplate2.degree;
		}
		static FunctionTerm Create(int degree)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			return Terms.Polynomial(Terms.StandardPolynomialBasis(degree), 2);
		}
		static int Create(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			return (int)source.Element("degree");
		}
	}
}

