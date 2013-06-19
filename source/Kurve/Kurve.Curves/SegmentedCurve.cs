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

		public SegmentedCurve(IEnumerable<Segment> segments)
		{
			if (segments == null) throw new ArgumentNullException("segments");

			this.segments = segments;
		}

		public override Vector2Double GetPoint(double position)
		{
			return 
			(
				from segment in segments
				where segment.Contains(position)
				select segment.GlobalCurve.GetPoint(position)
			)
			.Distinct()
			.Single();
		}
		public override Vector2Double GetVelocity(double position)
		{
			return 
			(
				from segment in segments
				where segment.Contains(position)
				select segment.GlobalCurve.GetVelocity(position)
			)
			.Distinct()
			.Single();
		}
		public override Vector2Double GetAcceleration(double position)
		{
			return 
			(
				from segment in segments
				where segment.Contains(position)
				select segment.GlobalCurve.GetAcceleration(position)
			)
			.Distinct()
			.Single();
		}
	}
}

