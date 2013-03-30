using System;
using Kurve.Ipopt;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Calculus.Terms;
using Krach.Calculus;
using Krach.Calculus.Terms.Composite;

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
				Term.Exponentiation(Term.Difference(Term.Constant(1), x), Term.Constant(2)),
				Term.Product(Term.Constant(100), Term.Exponentiation(Term.Difference(y, Term.Exponentiation(x, Term.Constant(2))), Term.Constant(2)))
			)
			.Abstract(x, y);

			//function.Normalize(2);
			//rosenbrock.Normalize(2);

			//Optimize(function, Enumerables.Create(1.0, 1.0));
			Optimize(rosenbrock, Enumerables.Create(-1.2, 1.0));

			//function.Simplify(2, false);
			//rosenbrock.Simplify(2, false);
		}
		static void Optimize(FunctionTerm function, IEnumerable<double> startPosition)
		{
			Problem problem = new Problem(function.Normalize(2), Constraint.CreateEmpty(function.DomainDimension), new Settings());
			
			IEnumerable<double> resultPosition = problem.Solve(startPosition);

			Console.WriteLine("function: {0}", function);
			Console.WriteLine("start position: {0}", startPosition.ToStrings().Separate(", ").AggregateString());
			Console.WriteLine("result position: {0}", resultPosition.ToStrings().Separate(", ").AggregateString());
		}
	}
}
