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
		readonly double speed;
		readonly Vector2Double direction;
		readonly double curvature;

		public Vector2Double Point { get { return point; } }
		public Vector2Double Velocity { get { return velocity; } }
		public Vector2Double Acceleration { get { return acceleration; } }
		public double Speed { get { return speed; } }
		public Vector2Double Direction { get { return direction; } }
		public double Curvature { get { return curvature; } }

		public DiscreteCurveItem(Vector2Double point, Vector2Double velocity, Vector2Double acceleration, double speed, Vector2Double direction, double curvature)
		{
			this.point = point;
			this.velocity = velocity;
			this.acceleration = acceleration;
			this.speed = speed;
			this.direction = direction;
			this.curvature = curvature;
		}
	}
}

