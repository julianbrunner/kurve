using System;
using Kurve.Ipopt;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Calculus.Terms;
using Krach.Calculus;
using Krach.Calculus.Terms.Combination;

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
				Enumerables.Create
				(
					Term.Product(Term.Constant(+5), Term.Product(x, x)),
					Term.Product(Term.Constant(-1), Term.Product(x, y)),
	   				Term.Product(Term.Constant(+1), Term.Product(y, y))
				)
			)
			.Abstract(x, y);
			
			FunctionTerm rosenbrock = Term.Sum
			(
				Term.Exponentiate(Term.Difference(Term.Constant(1), x), Term.Constant(2)),
				Term.Product(Term.Constant(100), Term.Exponentiate(Term.Difference(y, Term.Exponentiate(x, Term.Constant(2))), Term.Constant(2)))
			)
			.Abstract(x, y);
			
			Optimize(function, Enumerables.Create(1.0, 1.0));
			Optimize(rosenbrock, Enumerables.Create(-1.2, 1.0));
		}
		static void Optimize(FunctionTerm function, IEnumerable<double> startPosition)
		{
			Problem problem = new Problem(function.Simplify(2), new Constraint(function.DomainDimension), new Settings());
			
			IEnumerable<double> resultPosition = problem.Solve(startPosition);

			Console.WriteLine("function: {0}", function.GetText());
			Console.WriteLine("start position: {0}", startPosition.ToStrings().Separate(", ").AggregateString());
			Console.WriteLine("result position: {0}", resultPosition.ToStrings().Separate(", ").AggregateString());
		}
	}
}
