using System;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	public abstract class CurveSpecification
	{
		public abstract double Position { get; }

		public abstract ValueTerm GetErrorTerm(Curve curve);
	}
}
