using System;
using System.Linq;
using Kurve.Ipopt;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Kurve.Curves.Segmentation;
using Kurve.Curves.Specification;
using Krach.Calculus.Terms;
using Krach.Calculus.Terms.Composite;

namespace Kurve.Curves
{
	class Assignment
	{
		readonly Variable variable;
		readonly IEnumerable<double> value;

		public Variable Variable { get { return variable; } }
		public IEnumerable<double> Value { get { return value; } }

		public Assignment(Variable variable, IEnumerable<double> value)
		{
			if (variable == null) throw new ArgumentNullException("variable");
			if (value == null) throw new ArgumentNullException("value");

			this.variable = variable;
			this.value = value;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1}", variable, value.ToStrings().Separate(" ").AggregateString());
		}
	}
}

