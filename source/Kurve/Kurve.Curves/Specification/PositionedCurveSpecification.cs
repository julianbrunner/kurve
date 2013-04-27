using System;

namespace Kurve.Curves
{
	public abstract class PositionedCurveSpecification : CurveSpecification
	{
		public abstract double Position { get; }
	}
}
