using System;
using Krach.Basics;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Linq;

namespace Kurve.Curves
{
	public class PointCurveSpecification : CurveSpecification
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

		public override ValueTerm GetErrorTerm(Curve curve)
		{
			return Terms.Difference(curve.Point.Apply(Terms.Constant(position)), Terms.Constant(point.X, point.Y));
		}
	}
}
