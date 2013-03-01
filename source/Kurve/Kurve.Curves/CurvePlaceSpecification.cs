using System;
using Krach.Design;
using Krach.Basics;

namespace Kurve.Curves
{
	public class CurvePlaceSpecification
	{
		readonly double position;
		readonly Option<Vector2Double> point;
		readonly Option<Vector2Double> velocity;
		
		public double Position { get { return position; } }
		public Option<Vector2Double> Point { get { return point; } }
		public Option<Vector2Double> Velocity { get { return velocity; } }
		
		CurvePlaceSpecification(double position, Option<Vector2Double> point, Option<Vector2Double> velocity)
		{
			this.position = position;
			this.point = point;
			this.velocity = velocity;
		}
		
		public static CurvePlaceSpecification CreatePointSpecification(double position, Vector2Double point) 
		{
			return new CurvePlaceSpecification(position, new Option<Vector2Double>(point), null);		
		}
		public static CurvePlaceSpecification CreateVelocitySpecification(double position, Vector2Double velocity) 
		{
			return new CurvePlaceSpecification(position, null, new Option<Vector2Double>(velocity));		
		}
		public static CurvePlaceSpecification CreatePointVelocitySpecification(double position, Vector2Double point, Vector2Double velocity) 
		{
			return new CurvePlaceSpecification(position, new Option<Vector2Double>(point), new Option<Vector2Double>(velocity));		
		}
	}
}

