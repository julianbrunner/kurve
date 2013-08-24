using System;
using Krach.Basics;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Linq;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class DirectionCurveSpecification : CurveSpecification, IEquatable<DirectionCurveSpecification>
	{
		readonly double position;
		readonly double direction;
		
		public static string XElementName { get { return "direction_curve_specification"; } }

		public override double Position { get { return position; } }
		public double Direction { get { return direction; } }
		public override XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("position", position),
					new XElement("direction", direction)
				);	
			}
		}

		public DirectionCurveSpecification(double position, double direction)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.direction = direction;
		}
		public DirectionCurveSpecification(XElement source)
		{	
			if (source == null) throw new ArgumentNullException("source");

			this.position = (double)source.Element("position");
			this.direction = (double)source.Element("direction");
		}

		public override bool Equals(object obj)
		{
			return obj is DirectionCurveSpecification && Equals(this, (DirectionCurveSpecification)obj);
		}
		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode() ^ position.GetHashCode() ^ direction.GetHashCode();
		}
		public bool Equals(DirectionCurveSpecification other)
		{
			return object.Equals(this, other);
		}

		public static bool operator ==(DirectionCurveSpecification curveSpecification1, DirectionCurveSpecification curveSpecification2)
		{
			return object.Equals(curveSpecification1, curveSpecification2);
		}
		public static bool operator !=(DirectionCurveSpecification curveSpecification1, DirectionCurveSpecification curveSpecification2)
		{
			return !object.Equals(curveSpecification1, curveSpecification2);
		}
		
		static bool Equals(DirectionCurveSpecification curveSpecification1, DirectionCurveSpecification curveSpecification2) 
		{
			if (object.ReferenceEquals(curveSpecification1, curveSpecification2)) return true;
			if (object.ReferenceEquals(curveSpecification1, null) || object.ReferenceEquals(curveSpecification2, null)) return false;
			
			return curveSpecification1.position == curveSpecification2.position && curveSpecification1.direction == curveSpecification2.direction;
		}
	}
}
