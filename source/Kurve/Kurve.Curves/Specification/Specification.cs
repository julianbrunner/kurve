using System;
using Wrappers.Casadi;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Kurve.Curves
{
	public class Specification
	{
		readonly BasicSpecification basicSpecification;
		readonly IEnumerable<double> disambiguation;

		public static string XElementName { get { return "specification"; } }

		public BasicSpecification BasicSpecification { get { return basicSpecification; } }
		public IEnumerable<double> Disambiguation { get { return disambiguation; } }
		public XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("basic_specification", basicSpecification.XElement),
					new XElement("disambiguation", from parameter in disambiguation select new XElement("parameter", parameter))
				);
			} 
		}

		public Specification(BasicSpecification basicSpecification, IEnumerable<double> disambiguation)
		{
			if (basicSpecification == null) throw new ArgumentNullException("basicSpecification");
			if (disambiguation == null) throw new ArgumentNullException("disambiguation");

			this.basicSpecification = basicSpecification;
			this.disambiguation = disambiguation;
		}
		public Specification(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			this.basicSpecification = new BasicSpecification(source.Element("basic_specification").Elements().Single());
			this.disambiguation =
			(
				from element in source.Element("disambiguation").Elements("parameter")
				select (double)element
			)
			.ToArray();
		}
	}
}
