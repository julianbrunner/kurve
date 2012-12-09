using System;
using Krach.Basics;

namespace Kurve.Curves
{
	public class CurvePlaceSpecification
	{
		readonly Vector2Double position;

		public Vector2Double Position { get { return position; } }

		public CurvePlaceSpecification(Vector2Double position)
		{
			this.position = position;
		}
	}
}
