using System;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	public abstract class CurveSpecification
	{
		public abstract ValueTerm GetErrorTerm(Curve curve);
	}
}
