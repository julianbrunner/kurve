using System;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;

namespace Wrappers.Casadi
{
	public class Constraint<T>
	{        
		readonly T item;
		readonly IEnumerable<OrderedRange<double>> ranges;

		public T Item { get { return item; } }
		public IEnumerable<OrderedRange<double>> Ranges { get { return ranges; } }

		public Constraint(T item, IEnumerable<OrderedRange<double>> ranges)
		{
			if (item == null) throw new ArgumentNullException("function");
			if (ranges == null) throw new ArgumentNullException("ranges");

			this.item = item;
			this.ranges = ranges;
		}

		public override string ToString()
		{
			return string.Format("{0}: [{1}]", item, ranges.ToStrings().Separate(", ").AggregateString());
		}
	}
}

