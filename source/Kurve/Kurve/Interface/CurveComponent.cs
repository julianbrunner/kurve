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
		IEnumerable<Curve> segments;

		public IEnumerable<Curve> Segments
		{
			get { return segments; }
			set
			{
				segments = value;

				OnUpdate();
			}
		}

		public override void Draw(Context context)
		{
			if (segments == null) return;

			double stepLength = 0.01;
			Krach.Graphics.Color startColor = Colors.Red;
			Krach.Graphics.Color endColor = Colors.Blue;

			foreach (Curve segment in segments)
			{
				FunctionTerm segmentPoint = segment.Point;

				for (double position = 0; position < 1; position += stepLength)
				{
					Krach.Graphics.Color color = Krach.Graphics.Color.InterpolateHsv(startColor, endColor, Scalars.InterpolateLinear, position);

					InterfaceUtility.DrawLine(context, EvaluatePoint(segmentPoint, position), EvaluatePoint(segmentPoint, position + stepLength), 2, color);
				}
			}
		}

		static Vector2Double EvaluatePoint(FunctionTerm curve, double position)
		{
			IEnumerable<double> result = curve.Apply(Terms.Constant(position)).Evaluate();

			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
		}
	}
}

