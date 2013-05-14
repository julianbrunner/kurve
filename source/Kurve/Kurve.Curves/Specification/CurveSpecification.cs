using System;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Kurve.Curves
{
	public abstract class CurveSpecification
	{
		public abstract double Position { get; }
		public abstract XElement XElement { get; }

		public abstract ValueTerm GetErrorTerm(Curve curve);

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
	}
}
