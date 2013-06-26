using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Cairo;
using Krach.Graphics;
using Kurve.Curves;

namespace Kurve
{
	class DiscreteCurve : Curve
	{
		readonly IEnumerable<DiscreteCurveItem> items;

		public IEnumerable<DiscreteCurveItem> Items { get { return items; } }

		public DiscreteCurve(Curve curve)
		{
			if (curve == null) throw new ArgumentNullException("curve");

			this.items =
			(
				from position in Scalars.GetIntermediateValues(0, 1, 100)
				select new DiscreteCurveItem(curve.GetPoint(position), curve.GetVelocity(position), curve.GetAcceleration(position))
			)
			.ToArray();
		}

		public override Vector2Double GetPoint(double position)
		{
			return GetItem(position).Point;
		}
		public override Vector2Double GetVelocity(double position)
		{
			return GetItem(position).Velocity;
		}
		public override Vector2Double GetAcceleration(double position)
		{
			return GetItem(position).Acceleration;
		}

		DiscreteCurveItem GetItem(double position)
		{
			if (position == 1) return items.Last();

			return items.ElementAt((int)(position * items.Count()));
		}
	}
}

