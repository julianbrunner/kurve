using System;
using Krach.Basics;

namespace Kurve.Component
{
	class FixedPositionComponent : PositionedControlComponent
	{
		double position = 0;

		public override double Position { get { return position; } }

		public FixedPositionComponent(Component parent, CurveComponent curveComponent, double position) : base(parent, curveComponent)
		{
			this.position = position;
		}

		public override bool Contains(Vector2Double point)
		{
			return false;
		}
	}
}

