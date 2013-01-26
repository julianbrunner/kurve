using System;
using Krach.Basics;

namespace Kurve.Curves
{
	public class CurvePlaceSpecification
	{
		readonly Vector2Double point;
		readonly Vector2Double velocity;
		
		public Vector2Double Point { get { return point; } }
		public Vector2Double Velocity { get { return velocity; } }
		
		public CurvePlaceSpecification(Vector2Double point, Vector2Double velocity)
		{
			this.point = point;
			this.velocity = velocity;
		}
	}
}
