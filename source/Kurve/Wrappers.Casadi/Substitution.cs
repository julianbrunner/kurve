using System;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;

namespace Wrappers.Casadi
{
	public class Substitution
	{        
		readonly ValueTerm variable;
		readonly ValueTerm value;

		public ValueTerm Variable { get { return variable; } }
		public ValueTerm Value { get { return value; } }

		public Substitution(ValueTerm variable, ValueTerm value)
		{
			if (variable == null) throw new ArgumentNullException("variable");
			if (value == null) throw new ArgumentNullException("value");

			this.variable = variable;
			this.value = value;
		}
	}
}

