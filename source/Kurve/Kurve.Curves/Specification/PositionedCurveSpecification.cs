using System;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	public abstract class PositionedCurveSpecification : CurveSpecification
	{
		public abstract double Position { get; }
	}
}
