using System;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	abstract class CurveSpecification
	{
		public abstract double Position { get; }
		public abstract Term GetErrorTerm(CurvePoint curvePoint);
	}
}
