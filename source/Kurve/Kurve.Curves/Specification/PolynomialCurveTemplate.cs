using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Wrappers.Casadi;
using System.Xml.Linq;

namespace Kurve.Curves
{
	public class PolynomialCurveTemplate : CurveTemplate
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

		public PolynomialCurveTemplate(int degree) : base(Create(degree))
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			this.degree = degree;
		}
		public PolynomialCurveTemplate(XElement source) : this(Create(source)) { }

		static FunctionTerm Create(int degree)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");

			return Terms.Polynomial(2, degree);
		}
		static int Create(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			return (int)source.Element("degree");
		}
	}
}

