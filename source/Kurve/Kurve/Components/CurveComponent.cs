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
	class CurveComponent : Component
	{
		DiscreteCurve discreteCurve;

		public DiscreteCurve DiscreteCurve
		{
			get { return discreteCurve; }
			set
			{
				discreteCurve = value;

				OnUpdate();
			} 
		}

		public override void Draw(Context context)
		{
			if (discreteCurve == null) return;

			IEnumerable<Tuple<Vector2Double, Vector2Double>> segments = discreteCurve.Items.Select(item => item.Point).GetRanges().ToArray();

			for (int index = 0; index < segments.Count(); index++)
			{
				Krach.Graphics.Color color = Krach.Graphics.Color.InterpolateHsv(Colors.Red, Colors.Blue, Scalars.InterpolateLinear, (double)index / (double)segments.Count());

				InterfaceUtility.DrawLine(context, segments.ElementAt(index).Item1, segments.ElementAt(index).Item2, 2, color);
			}
		}
	}
}

