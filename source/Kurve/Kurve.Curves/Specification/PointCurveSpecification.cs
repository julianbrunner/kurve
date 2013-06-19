using System;
using Krach.Basics;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Linq;

namespace Kurve.Curves
{
	public class PointCurveSpecification : CurveSpecification, IEquatable<PointCurveSpecification>
	{
		readonly double position;
		readonly Vector2Double point;

		public static string XElementName { get { return "point_curve_specification"; } }

		public override double Position { get { return position; } }
		public Vector2Double Point { get { return point; } }
		public override XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("position", position),
					new XElement("point", point.XElement)
				);	
			}
		}
		
		public PointCurveSpecification(double position, Vector2Double point)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.point = point;
		}
		public PointCurveSpecification(XElement source)
		{	
			if (source == null) throw new ArgumentNullException("source");

			this.position = (double)source.Element("position");
			this.point = new Vector2Double(source.Element("point").Elements().Single());
		}

		public override bool Equals(object obj)
		{
			return obj is PointCurveSpecification && Equals(this, (PointCurveSpecification)obj);
		}
		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode() ^ position.GetHashCode() ^ point.GetHashCode();
		}
		public bool Equals(PointCurveSpecification other)
		{
			return object.Equals(this, other);
		}

		// TODO: clean up
		public static ValueTerm GetErrorTerm(FunctionTermCurve curve, ValueTerm position, ValueTerm point)
		{
			return Terms.Difference(curve.Point.Apply(position), point);
		}

		public static bool operator ==(PointCurveSpecification curveSpecification1, PointCurveSpecification curveSpecification2)
		{
			return object.Equals(curveSpecification1, curveSpecification2);
		}
		public static bool operator !=(PointCurveSpecification curveSpecification1, PointCurveSpecification curveSpecification2)
		{
			return !object.Equals(curveSpecification1, curveSpecification2);
		}
		
		static bool Equals(PointCurveSpecification curveSpecification1, PointCurveSpecification curveSpecification2) 
		{
			if (object.ReferenceEquals(curveSpecification1, curveSpecification2)) return true;
			if (object.ReferenceEquals(curveSpecification1, null) || object.ReferenceEquals(curveSpecification2, null)) return false;
			
			return curveSpecification1.position == curveSpecification2.position && curveSpecification1.point == curveSpecification2.point;
		}
	}
}
