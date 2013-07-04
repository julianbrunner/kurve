using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	class SegmentedCurve : Curve
	{
		readonly IEnumerable<Segment> segments;
		readonly Func<double, int> getSegmentIndex;

		public SegmentedCurve(IEnumerable<Segment> segments, Func<double, int> getSegmentIndex)
		{
			if (segments == null) throw new ArgumentNullException("segments");
			if (getSegmentIndex == null) throw new ArgumentNullException("getSegmentIndex");

			this.segments = segments;
			this.getSegmentIndex = getSegmentIndex;
		}

		public override Vector2Double GetPoint(double position)
		{
			return segments.ElementAt(getSegmentIndex(position)).GlobalCurve.GetPoint(position);
		}
		public override Vector2Double GetVelocity(double position)
		{
			return segments.ElementAt(getSegmentIndex(position)).GlobalCurve.GetVelocity(position);
		}
		public override Vector2Double GetAcceleration(double position)
		{
			return segments.ElementAt(getSegmentIndex(position)).GlobalCurve.GetAcceleration(position);
		}
		public override double GetSpeed(double position)
		{
			return segments.ElementAt(getSegmentIndex(position)).GlobalCurve.GetSpeed(position);
		}
		public override Vector2Double GetDirection(double position)
		{
			return segments.ElementAt(getSegmentIndex(position)).GlobalCurve.GetDirection(position);
		}
		public override double GetCurvature(double position)
		{
			return segments.ElementAt(getSegmentIndex(position)).GlobalCurve.GetCurvature(position);
		}
	}
}

