using System;
using Krach.Calculus.Terms;
using Krach.Extensions;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Kurve.Ipopt;
using Krach.Calculus;

namespace Kurve.Curves.Segmentation
{
	class CurveConstraint
	{
		readonly ValueTerm value;
		readonly IEnumerable<OrderedRange<double>> ranges;

		public ValueTerm Value { get { return value; } }
		public IEnumerable<OrderedRange<double>> Ranges { get { return ranges; } }

		CurveConstraint(ValueTerm value, IEnumerable<OrderedRange<double>> ranges)
		{
			if (value == null) throw new ArgumentNullException("value");
			if (ranges == null) throw new ArgumentNullException("ranges");
			
			this.value = value;
			this.ranges = ranges;
		}

		public static CurveConstraint FromPointConnection(Segment end, Segment start)
		{
			return FromEquality
			(
				end.GetLocalCurve().Function.Apply(Term.Constant(1)),
				start.GetLocalCurve().Function.Apply(Term.Constant(0))
			);
		}
		public static CurveConstraint FromVelocityConnection(Segment end, Segment start)
		{
			return FromEquality
			(
				end.GetLocalCurve().Derivative.Function.Apply(Term.Constant(0)),
				start.GetLocalCurve().Derivative.Function.Apply(Term.Constant(1))
			);
		}

		static CurveConstraint FromEquality(ValueTerm value1, ValueTerm value2)
		{
			int dimension = Items.Equal(value1.Dimension, value2.Dimension);

			return new CurveConstraint
			(
				Term.Difference(value1, value2),
				Enumerable.Repeat(new OrderedRange<double>(0), dimension)
			);
		}
	}
}

