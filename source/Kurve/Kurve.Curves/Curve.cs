using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	public abstract class Curve
	{
		public abstract Vector2Double GetPoint(double position);
		public abstract Vector2Double GetVelocity(double position);
		public abstract Vector2Double GetAcceleration(double position);
		public abstract double GetSpeed(double position);
		public abstract Vector2Double GetDirection(double position);
		public abstract double GetCurvature(double position);
	}
}

