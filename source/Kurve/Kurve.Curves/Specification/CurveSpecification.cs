using System;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Kurve.Curves
{
	public abstract class CurveSpecification : IEquatable<CurveSpecification>
	{
		public abstract double Position { get; }
		public abstract XElement XElement { get; }

		public override bool Equals(object obj)
		{
			throw new InvalidOperationException();
		}
		public override int GetHashCode()
		{
			throw new InvalidOperationException();
		}
		public bool Equals(CurveSpecification other)
		{
			return object.Equals(this, other);
		}

		public static CurveSpecification Parse(XElement element)
		{
			switch (element.Name.ToString()) 
			{
				case "point_curve_specification": return new PointCurveSpecification(element);
				case "velocity_curve_specification": return new VelocityCurveSpecification(element);
				case "acceleration_curve_specification": return new AccelerationCurveSpecification(element);
				default: throw new ArgumentException("Parameter 'element' is not a CurveSpecification.");
			}
		}
		
		public static bool operator ==(CurveSpecification curveSpecification1, CurveSpecification curveSpecification2)
		{
			return object.Equals(curveSpecification1, curveSpecification2);
		}
		public static bool operator !=(CurveSpecification curveSpecification1, CurveSpecification curveSpecification2)
		{
			return !object.Equals(curveSpecification1, curveSpecification2);
		}
	}
}
