using System;
using Krach.Basics;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Linq;

namespace Kurve.Curves
{
	public class CurvatureCurveSpecification : CurveSpecification, IEquatable<CurvatureCurveSpecification>
	{
		readonly double position;
		readonly double curvature;
		
		public static string XElementName { get { return "curvature_curve_specification"; } }

		public override double Position { get { return position; } }
		public double Curvature { get { return curvature; } }
		public override XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("position", position),
					new XElement("curvature", curvature)
				);	
			}
		}
		
		public CurvatureCurveSpecification(double position, double curvature)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.curvature = curvature;
		}
		public CurvatureCurveSpecification(XElement source)
		{	
			if (source == null) throw new ArgumentNullException("source");

			this.position = (double)source.Element("position");
			this.curvature = (double)source.Element("curvature");
		}

		public override bool Equals(object obj)
		{
			return obj is CurvatureCurveSpecification && Equals(this, (CurvatureCurveSpecification)obj);
		}
		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode() ^ position.GetHashCode() ^ curvature.GetHashCode();
		}
		public bool Equals(CurvatureCurveSpecification other)
		{
			return object.Equals(this, other);
		}

		public static bool operator ==(CurvatureCurveSpecification curveSpecification1, CurvatureCurveSpecification curveSpecification2)
		{
			return object.Equals(curveSpecification1, curveSpecification2);
		}
		public static bool operator !=(CurvatureCurveSpecification curveSpecification1, CurvatureCurveSpecification curveSpecification2)
		{
			return !object.Equals(curveSpecification1, curveSpecification2);
		}
		
		static bool Equals(CurvatureCurveSpecification curveSpecification1, CurvatureCurveSpecification curveSpecification2) 
		{
			if (object.ReferenceEquals(curveSpecification1, curveSpecification2)) return true;
			if (object.ReferenceEquals(curveSpecification1, null) || object.ReferenceEquals(curveSpecification2, null)) return false;
			
			return curveSpecification1.position == curveSpecification2.position && curveSpecification1.curvature == curveSpecification2.curvature;
		}
	}
}
