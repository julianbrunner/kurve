using System;
using Krach.Calculus.Terms;
using Krach.Basics;

namespace Kurve.Curves
{
	class Constraint
	{
		readonly Term term;
		readonly OrderedRange<double> range;
		
		public Term Term { get { return term; } }
		public OrderedRange<double> Range { get { return range; } }
		
		public Constraint(Term term, OrderedRange<double> range)
		{
			if (term == null) throw new ArgumentNullException("term");
			
			this.term = term;
			this.range = range;
		}
	}
}

