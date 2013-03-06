using System;
using Krach.Basics;
using Krach.Terms;

namespace Kurve.Ipopt
{
	public class Constraint
	{
		readonly Function function;
		readonly OrderedRange<double> range;
		
		public Function Function { get { return function; } }
		public OrderedRange<double> Range { get { return range; } }
		
		public Constraint(Function function, OrderedRange<double> range)
		{
			if (function == null) throw new ArgumentNullException("function");
			
			this.function = function;
			this.range = range;
		}

	}
}

