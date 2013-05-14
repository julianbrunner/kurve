using System;
using Krach.Basics;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Linq;

namespace Kurve.Curves
{
	public class AccelerationCurveSpecification : CurveSpecification
	{
		readonly double position;
		readonly Vector2Double acceleration;
		
		public static string XElementName { get { return "acceleration_curve_specification"; } }

		public override double Position { get { return position; } }
		public Vector2Double Acceleration { get { return acceleration; } }
		public override XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("position", position),
					new XElement("acceleration", acceleration.XElement)
				);	
			}
		}
		
		public AccelerationCurveSpecification(double position, Vector2Double acceleration)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.acceleration = acceleration;
		}
		public AccelerationCurveSpecification(XElement source)
		{	
			if (source == null) throw new ArgumentNullException("source");

			this.position = (double)source.Element("position");
			this.acceleration = new Vector2Double(source.Element("acceleration").Elements().Single());
		}

		public override ValueTerm GetErrorTerm(Curve curve)
		{
			return Terms.Difference(curve.Acceleration.Apply(Terms.Constant(position)), Terms.Constant(acceleration.X, acceleration.Y));
		}
	}
}
