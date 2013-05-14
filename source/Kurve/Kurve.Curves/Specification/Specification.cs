using System;
using Wrappers.Casadi;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Kurve.Curves
{
	public class Specification
	{
		readonly double curveLength;
		readonly int segmentCount;
		readonly CurveTemplate segmentTemplate;
		readonly IEnumerable<CurveSpecification> curveSpecifications;
		readonly IEnumerable<double> disambiguation;

		public static string XElementName { get { return "specification"; } }

		public double CurveLength { get { return curveLength; } }
		public int SegmentCount { get { return segmentCount; } }
		public CurveTemplate SegmentTemplate { get { return segmentTemplate; } }
		public IEnumerable<CurveSpecification> CurveSpecifications { get { return curveSpecifications; } }
		public IEnumerable<double> Disambiguation { get { return disambiguation; } }
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
					new XElement("curve_specifications", from curveSpecification in curveSpecifications select curveSpecification.XElement),
					new XElement("disambiguation", from parameter in disambiguation select new XElement("parameter", parameter))
				);
			} 
		}

		public Specification(double curveLength, int segmentCount, CurveTemplate segmentTemplate, IEnumerable<CurveSpecification> curveSpecifications, IEnumerable<double> disambiguation)
		{
			if (curveLength < 0) throw new ArgumentOutOfRangeException("curveLength");
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");
			if (segmentTemplate == null) throw new ArgumentNullException("segmentTemplate");
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");
			if (disambiguation == null) throw new ArgumentNullException("disambiguation");

			this.curveLength = curveLength;
			this.segmentCount = segmentCount;
			this.segmentTemplate = segmentTemplate;
			this.curveSpecifications = curveSpecifications;
			this.disambiguation = disambiguation;
		}
		public Specification(XElement source)
		{
			if (source == null) throw new ArgumentNullException("source");

			this.curveLength = (double)source.Element("curve_length");
			this.segmentCount = (int)source.Element("segment_count");
			this.segmentTemplate = CurveTemplate.Parse(source.Element("segment_template").Elements().Single());
			this.curveSpecifications = source.Element("curve_specifications").Elements().Select(CurveSpecification.Parse).ToArray();
			this.disambiguation =
			(
				from element in source.Element("disambiguation").Elements("parameter")
				select (double)element
			)
			.ToArray();
		}
	}
}
