using System;
using Krach.Basics;
using System.Collections.Generic;

namespace Kurve.Curves
{
	public abstract class ParametricCurveClassSpecification
	{
		public abstract int ParameterCount { get; }

		public abstract ParametricCurve CreateCurve(IEnumerable<double> parameters);
	}
}

