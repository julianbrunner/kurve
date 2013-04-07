using System;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	public abstract class CurveSpecification
	{
		public abstract ValueTerm GetErrorTerm(Curve curve);
	}
}
