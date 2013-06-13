using System;
using Wrappers.Casadi;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class Specification : IEquatable<Specification>
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

		public override bool Equals(object obj)
		{
			return obj is Specification && Equals(this, (Specification)obj);
		}
		public override int GetHashCode()
		{
			return basicSpecification.GetHashCode() ^ Enumerables.GetSequenceHashCode(position);
		}
		public bool Equals(Specification other)
		{
			return object.Equals(this, other);
		}

		public static bool operator ==(Specification specification1, Specification specification2)
		{
			return object.Equals(specification1, specification2);
		}
		public static bool operator !=(Specification specification1, Specification specification2)
		{
			return !object.Equals(specification1, specification2);
		}
		
		static bool Equals(Specification specification1, Specification specification2) 
		{
			if (object.ReferenceEquals(specification1, specification2)) return true;
			if (object.ReferenceEquals(specification1, null) || object.ReferenceEquals(specification2, null)) return false;

			return
				specification1.basicSpecification == specification2.basicSpecification &&
				Enumerable.SequenceEqual(specification1.position, specification2.position);
		}
	}
}
