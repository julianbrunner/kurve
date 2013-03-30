using System;
using Krach.Basics;
using Krach.Calculus;
using System.Collections.Generic;
using Krach.Calculus.Terms;
using Krach.Extensions;
using System.Linq;
using Krach.Calculus.Terms.Composite;

namespace Kurve.Ipopt
{
	public class Constraint
	{
		readonly IFunction function;
		readonly IEnumerable<OrderedRange<double>> ranges;
		
		public IFunction Function { get { return function; } }
		public IEnumerable<OrderedRange<double>> Ranges { get { return ranges; } }
		
		public Constraint(IFunction function, IEnumerable<OrderedRange<double>> ranges)
		{
			if (function == null) throw new ArgumentNullException("function");
			if (ranges == null) throw new ArgumentNullException("ranges");
			
			this.function = function;
			this.ranges = ranges;
		}

		public static Constraint CreateEmpty(int dimension)
		{
			return new Constraint
			(
				Term.Vector().Abstract(new Variable(dimension, "x")),
				Enumerables.Create<OrderedRange<double>>()
			);
		}
	}
}

