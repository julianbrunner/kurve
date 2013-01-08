using System;
using Krach.Basics;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	public class ParametricCurve
	{
		readonly Term x;
		readonly Term y;

		public static Variable Position { get { return new Variable("position"); } }

		public Term X { get { return x; } }
		public Term Y { get { return y; } }

		public ParametricCurve(Term x, Term y)
		{
			if (x == null) throw new ArgumentNullException("x");
			if (y == null) throw new ArgumentNullException("y");

			this.x = x;
			this.y = y;
		}
	}
}

