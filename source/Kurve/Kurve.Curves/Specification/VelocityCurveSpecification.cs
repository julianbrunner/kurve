using System;
using Krach.Basics;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Linq;

namespace Kurve.Curves
{
	public class VelocityCurveSpecification : CurveSpecification
	{
		readonly double position;
		readonly Vector2Double velocity;
		
		public static string XElementName { get { return "velocity_curve_specification"; } }

		public override double Position { get { return position; } }
		public Vector2Double Direction { get { return velocity; } }
		public override XElement XElement
		{
			get
			{
				return new XElement
				(
					XElementName,
					new XElement("position", position),
					new XElement("velocity", velocity.XElement)
				);	
			}
		}

		public VelocityCurveSpecification(double position, Vector2Double velocity)
		{
			if (position < 0 || position > 1) throw new ArgumentOutOfRangeException("position");
			
			this.position = position;
			this.velocity = velocity;
		}
		public VelocityCurveSpecification(XElement source)
		{	
			if (source == null) throw new ArgumentNullException("source");

			this.position = (double)source.Element("position");
			this.velocity = new Vector2Double(source.Element("velocity").Elements().Single());
		}

		public override ValueTerm GetErrorTerm(Curve curve)
		{
			return Terms.Difference(curve.Velocity.Apply(Terms.Constant(position)), Terms.Constant(velocity.X, velocity.Y));
		}
	}
}
