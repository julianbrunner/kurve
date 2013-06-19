using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Cairo;
using Krach.Graphics;
using Kurve.Curves;

namespace Kurve.Interface
{
	class PolygonComponent : Component
	{
		readonly IEnumerable<Vector2Double> points;

		public PolygonComponent(IEnumerable<Vector2Double> points)
		{
			if (points == null) throw new ArgumentNullException("points");

			this.points = points;
		}

		public override void Draw(Context context)
		{
			IEnumerable<Tuple<Vector2Double, Vector2Double>> segments = points.GetRanges().ToArray();

			for (int index = 0; index < segments.Count(); index++)
			{
				Krach.Graphics.Color color = Krach.Graphics.Color.InterpolateHsv(Colors.Red, Colors.Blue, Scalars.InterpolateLinear, (double)index / (double)segments.Count());

				InterfaceUtility.DrawLine(context, segments.ElementAt(index).Item1, segments.ElementAt(index).Item2, 2, color);
			}
		}
	}
}

