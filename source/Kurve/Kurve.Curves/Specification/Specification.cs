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
		readonly IEnumerable<double> position;

		public static string XElementName { get { return "specification"; } }

		public BasicSpecification BasicSpecification { get { return basicSpecification; } }
		public IEnumerable<double> Position { get { return position; } }
		public XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("basic_specification", basicSpecification.XElement),
					new XElement("position", from parameter in position select new XElement("parameter", parameter))
				);
			} 
		}

		public Specification(BasicSpecification basicSpecification, IEnumerable<double> position)
		{
			if (basicSpecification == null) throw new ArgumentNullException("basicSpecification");
			if (position == null) throw new ArgumentNullException("position");

			this.basicSpecification = basicSpecification;
			this.position = position;
		}
		public Specification(BasicSpecification basicSpecification)
		{
			if (basicSpecification == null) throw new ArgumentNullException("basicSpecification");

			this.basicSpecification = basicSpecification;
			this.position =
			(
				from segmentIndex in Enumerable.Range(0, basicSpecification.SegmentCount)
				from parameterIndex in Enumerable.Range(0, basicSpecification.SegmentTemplate.ParameterDimension)
				select 1.0
			)
			.ToArray();
		}
		public Specification() : this(new BasicSpecification()) { }
		public Specification(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			this.basicSpecification = new BasicSpecification(source.Element("basic_specification").Elements().Single());
			this.position =
			(
				from element in source.Element("position").Elements("parameter")
				select (double)element
			)
			.ToArray();
		}
	}
}
