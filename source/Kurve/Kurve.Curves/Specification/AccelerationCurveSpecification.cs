using System;
using Krach.Basics;
using Wrappers.Casadi;
using System.Xml.Linq;
using System.Linq;

namespace Kurve.Curves
{
	public class AccelerationCurveSpecification : CurveSpecification, IEquatable<AccelerationCurveSpecification>
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

		public override bool Equals(object obj)
		{
			return obj is AccelerationCurveSpecification && Equals(this, (AccelerationCurveSpecification)obj);
		}
		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode() ^ position.GetHashCode() ^ acceleration.GetHashCode();
		}
		public bool Equals(AccelerationCurveSpecification other)
		{
			return object.Equals(this, other);
		}

//		public override ValueTerm GetErrorTerm(Curve curve)
//		{
//			return Terms.Difference(curve.Acceleration.Apply(Terms.Constant(position)), Terms.Constant(acceleration.X, acceleration.Y));
//		}

		public static bool operator ==(AccelerationCurveSpecification curveSpecification1, AccelerationCurveSpecification curveSpecification2)
		{
			return object.Equals(curveSpecification1, curveSpecification2);
		}
		public static bool operator !=(AccelerationCurveSpecification curveSpecification1, AccelerationCurveSpecification curveSpecification2)
		{
			return !object.Equals(curveSpecification1, curveSpecification2);
		}
		
		static bool Equals(AccelerationCurveSpecification curveSpecification1, AccelerationCurveSpecification curveSpecification2) 
		{
			if (object.ReferenceEquals(curveSpecification1, curveSpecification2)) return true;
			if (object.ReferenceEquals(curveSpecification1, null) || object.ReferenceEquals(curveSpecification2, null)) return false;
			
			return curveSpecification1.position == curveSpecification2.position && curveSpecification1.acceleration == curveSpecification2.acceleration;
		}
	}
}
