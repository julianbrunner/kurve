using System;
using Kurve.Ipopt;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Calculus;
using Krach.Calculus.Terms;
using Krach.Calculus.Terms.Composite;
using Krach.Calculus.Terms.Constraints;
using Kurve.Curves;
using Krach.Calculus.Abstract;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
        {
			Variable x = new Variable(1, "x");
			Variable y = new Variable(1, "y");

			FunctionTerm function = Term.Sum
			(
				Term.Product(Term.Constant(+5), x, x),
				Term.Product(Term.Constant(-1), x, y),
				Term.Product(Term.Constant(+1), y, y)
			)
			.Abstract(x, y);

			FunctionTerm rosenbrock = Term.Sum
			(
				Term.Square(Term.Difference(Term.Constant(1), x)),
				Term.Product(Term.Constant(100), Term.Square(Term.Difference(y, Term.Square(x))))
			)
			.Abstract(x, y);

			Optimize(function, Enumerables.Create(1.0, 1.0));
			Optimize(rosenbrock, Enumerables.Create(-1.2, 1.0));
		}
		static void Optimize(FunctionTerm objectiveFunction, IEnumerable<double> startPosition)
		{
			Variable position = new Variable(objectiveFunction.DomainDimension, "x");
			IConstraint<FunctionTerm> constraint = Constraint.CreateEmpty().Abstract(position);

			Problem problem = new Problem(objectiveFunction.Normalize(2), constraint.Normalize(2), new Settings());
			
			IEnumerable<double> resultPosition = problem.Solve(startPosition);

			Console.WriteLine("function: {0}", objectiveFunction);
			Console.WriteLine("start position: {0}", startPosition.ToStrings().Separate(", ").AggregateString());
			Console.WriteLine("result position: {0}", resultPosition.ToStrings().Separate(", ").AggregateString());
		}
	}
}
