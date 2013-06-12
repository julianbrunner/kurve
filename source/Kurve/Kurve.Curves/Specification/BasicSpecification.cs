using System;
using Wrappers.Casadi;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class BasicSpecification
	{
		readonly double curveLength;
		readonly int segmentCount;
		readonly CurveTemplate segmentTemplate;
		readonly IEnumerable<CurveSpecification> curveSpecifications;

		public static string XElementName { get { return "basic_specification"; } }

		public double CurveLength { get { return curveLength; } }
		public int SegmentCount { get { return segmentCount; } }
		public CurveTemplate SegmentTemplate { get { return segmentTemplate; } }
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

		public BasicSpecification(double curveLength, int segmentCount, CurveTemplate segmentTemplate, IEnumerable<CurveSpecification> curveSpecifications)
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
		public BasicSpecification() : this(1, 1, new PolynomialCurveTemplate(1), Enumerables.Create<CurveSpecification>()) { }
		public BasicSpecification(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			this.curveLength = (double)source.Element("curve_length");
			this.segmentCount = (int)source.Element("segment_count");
			this.segmentTemplate = CurveTemplate.Parse(source.Element("segment_template").Elements().Single());
			this.curveSpecifications = source.Element("curve_specifications").Elements().Select(CurveSpecification.Parse).ToArray();
		}
	}
}
