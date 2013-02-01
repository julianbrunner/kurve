using System;
using Krach.Basics;

namespace Kurve.Curves
{
	public class CurvePlaceSpecification
	{
		readonly Vector2Double point;
		readonly Vector2Double direction;
		
		public Vector2Double Point { get { return point; } }
		public Vector2Double Direction { get { return direction; } }
		
		public CurvePlaceSpecification(Vector2Double point, Vector2Double direction)
		{
			this.point = point;
			this.direction = direction;
		}
	}
}
