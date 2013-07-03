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
	class DiscreteCurveItem
	{
		readonly Vector2Double point;
		readonly Vector2Double velocity;
		readonly Vector2Double acceleration;

		public Vector2Double Point { get { return point; } }
		public Vector2Double Velocity { get { return velocity; } }
		public Vector2Double Acceleration { get { return acceleration; } }

		public DiscreteCurveItem(Vector2Double point, Vector2Double velocity, Vector2Double acceleration)
		{
			this.point = point;
			this.velocity = velocity;
			this.acceleration = acceleration;
		}
	}
}

