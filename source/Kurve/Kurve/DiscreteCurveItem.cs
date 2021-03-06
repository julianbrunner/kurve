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
		readonly double speed;
		readonly double direction;
		readonly double curvature;

		public Vector2Double Point { get { return point; } }
		public double Speed { get { return speed; } }
		public double Direction { get { return direction; } }
		public double Curvature { get { return curvature; } }

		public DiscreteCurveItem(Vector2Double point, double speed, double direction, double curvature)
		{
			this.point = point;
			this.speed = speed;
			this.direction = direction;
			this.curvature = curvature;
		}
	}
}

