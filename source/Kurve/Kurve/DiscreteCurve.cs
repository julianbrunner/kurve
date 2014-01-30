using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Cairo;
using Krach.Graphics;
using Kurve.Curves;

namespace Kurve
{
	class DiscreteCurve : Curve
	{
		readonly IEnumerable<DiscreteCurveItem> items;

		public IEnumerable<DiscreteCurveItem> Items { get { return items; } }

		public DiscreteCurve(Curve curve, int segmentCount)
		{
			if (curve == null) throw new ArgumentNullException("curve");
			if (segmentCount < 1) throw new ArgumentOutOfRangeException("segmentCount");

			this.items =
			(
				from position in Scalars.GetIntermediateValuesSymmetric(0, 1, segmentCount + 1)
				select new DiscreteCurveItem
				(
					curve.GetPoint(position),
					curve.GetSpeed(position),
					curve.GetDirection(position),
					curve.GetCurvature(position)
				)
			)
			.ToArray();
		}

		public override Vector2Double GetPoint(double position)
		{
			return GetItem(position).Point;
		}
		public override double GetSpeed(double position)
		{
			return GetItem(position).Speed;
		}
		public override double GetDirection(double position)
		{
			return GetItem(position).Direction;
		}
		public override double GetCurvature(double position)
		{
			return GetItem(position).Curvature;
		}

		DiscreteCurveItem GetItem(double position)
		{
			return items.ElementAt(((int)(position * items.Count()).Round()).Clamp(0, items.Count() - 1));
		}
	}
}

