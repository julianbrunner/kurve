using System;
using Wrappers.Casadi;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class BasicSpecification : IEquatable<BasicSpecification>
	{
		readonly double curveLength;
		readonly int segmentCount;
		readonly FunctionTermCurveTemplate segmentTemplate;
		readonly IEnumerable<CurveSpecification> curveSpecifications;

		public static string XElementName { get { return "basic_specification"; } }

		public double CurveLength { get { return curveLength; } }
		public int SegmentCount { get { return segmentCount; } }
		public FunctionTermCurveTemplate SegmentTemplate { get { return segmentTemplate; } }
		public IEnumerable<CurveSpecification> CurveSpecifications { get { return curveSpecifications; } }
		public XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("curve_length", curveLength),
					new XElement("segment_count", segmentCount),
					new XElement("segment_template", segmentTemplate.XElement),
					new XElement("curve_specifications", from curveSpecification in curveSpecifications select curveSpecification.XElement)
				);
			} 
		}

		public BasicSpecification(double curveLength, int segmentCount, FunctionTermCurveTemplate segmentTemplate, IEnumerable<CurveSpecification> curveSpecifications)
		{
			if (curveLength < 0) throw new ArgumentOutOfRangeException("curveLength");
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");
			if (segmentTemplate == null) throw new ArgumentNullException("segmentTemplate");
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");

			this.curveLength = curveLength;
			this.segmentCount = segmentCount;
			this.segmentTemplate = segmentTemplate;
			this.curveSpecifications = curveSpecifications;
		}
		public BasicSpecification() : this(1, 1, new PolynomialFunctionTermCurveTemplate(1), Enumerables.Create<CurveSpecification>()) { }
		public BasicSpecification(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			this.curveLength = (double)source.Element("curve_length");
			this.segmentCount = (int)source.Element("segment_count");
			this.segmentTemplate = FunctionTermCurveTemplate.Parse(source.Element("segment_template").Elements().Single());
			this.curveSpecifications = source.Element("curve_specifications").Elements().Select(CurveSpecification.Parse).ToArray();
		}

		public override bool Equals(object obj)
		{
			return obj is BasicSpecification && Equals(this, (BasicSpecification)obj);
		}
		public override int GetHashCode()
		{
			return curveLength.GetHashCode() ^ segmentCount.GetHashCode() ^ segmentTemplate.GetHashCode() ^ Enumerables.GetSequenceHashCode(curveSpecifications);
		}
		public bool Equals(BasicSpecification other)
		{
			return object.Equals(this, other);
		}

		public static bool operator ==(BasicSpecification basicSpecification1, BasicSpecification basicSpecification2)
		{
			return object.Equals(basicSpecification1, basicSpecification2);
		}
		public static bool operator !=(BasicSpecification basicSpecification1, BasicSpecification basicSpecification2)
		{
			return !object.Equals(basicSpecification1, basicSpecification2);
		}
		
		static bool Equals(BasicSpecification basicSpecification1, BasicSpecification basicSpecification2) 
		{
			if (object.ReferenceEquals(basicSpecification1, basicSpecification2)) return true;
			if (object.ReferenceEquals(basicSpecification1, null) || object.ReferenceEquals(basicSpecification2, null)) return false;
			
			return
				basicSpecification1.curveLength == basicSpecification2.curveLength &&
				basicSpecification1.segmentCount == basicSpecification2.segmentCount &&
				basicSpecification1.segmentTemplate == basicSpecification2.segmentTemplate &&
				Enumerable.SequenceEqual(basicSpecification1.curveSpecifications, basicSpecification2.curveSpecifications);
		}
	}
}
