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
		readonly int length;

		public static string XElementName { get { return "polynomial_curve_template"; } }

		public int Degree { get { return length; } }
		public override XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("length", length)
				);
			}
		}

		public PolynomialFunctionTermCurveTemplate(int length) : base(Create(length))
		{
			if (length < 0) throw new ArgumentOutOfRangeException("length");

			this.length = length;
		}
		public PolynomialFunctionTermCurveTemplate(XElement source) : this(Create(source)) { }

		public override bool Equals(object obj)
		{
			return obj is PolynomialFunctionTermCurveTemplate && Equals(this, (PolynomialFunctionTermCurveTemplate)obj);
		}
		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode() ^ length.GetHashCode();
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
			
			return curveTemplate1.length == curveTemplate2.length;
		}
		static FunctionTerm Create(int length)
		{
			if (length < 0) throw new ArgumentOutOfRangeException("length");

			return Terms.Polynomial(Terms.StandardPolynomialBasis(length), 2);
		}
		static int Create(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			return (int)source.Element("degree");
		}
	}
}

