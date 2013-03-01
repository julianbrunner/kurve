using System;
using System.Collections.Generic;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	abstract class VirtualObject
	{
		public abstract IEnumerable<Variable> Variables { get; }
		public abstract IEnumerable<Constraint> Constraints { get; }
	}
}
