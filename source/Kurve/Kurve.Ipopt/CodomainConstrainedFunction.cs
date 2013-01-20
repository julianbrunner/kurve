using System;
using Krach.Basics;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Calculus;

namespace Kurve.Ipopt
{
	public class CodomainConstrainedFunction
	{
		readonly Function function;
		readonly Range<Matrix> constraints;

		public Function Function { get { return function; } }
		public Range<Matrix> Constraints { get { return constraints; } }

		public CodomainConstrainedFunction(Function function, Range<Matrix> constraints)
		{
			if (function == null) throw new ArgumentNullException("constraintsFunction");

			Vector2Integer constraintsSize = Items.Equal(constraints.Start.Size, constraints.End.Size);
			if (constraintsSize.X != function.CodomainDimension) throw new ArgumentException("Parameter constraints doesn't match codomain dimension of function.");
			if (constraintsSize.Y != 1) throw new ArgumentException("Parameter constraints is not a column vector range.");

			this.function = function;
			this.constraints = constraints;
		}
	}
}

