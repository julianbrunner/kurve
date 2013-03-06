using System;
using Kurve.Ipopt;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using Krach.Terms;
using Krach.Terms.LambdaTerms;
using Krach.Terms.Rewriting;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
		{
			//Console.WriteLine(Optimize(new QuadradicFunction(), Matrix.FromRowVectors(Enumerables.Create(1.0, 1.0).Select(Matrix.CreateSingleton))));
			//Console.WriteLine(Optimize(new Rosenbrock(), Matrix.FromRowVectors(Enumerables.Create(-1.2, 1.0).Select(Matrix.CreateSingleton))));
			//Console.WriteLine(Optimize(new RosenbrockSymbolic(), Matrix.FromRowVectors(Enumerables.Create(-1.2, 1.0).Select(Matrix.CreateSingleton))));
			//SymbolicFunction function = new QuadraticSymbolicFunction();
			
			Variable x = new Variable("x");
			Variable y = new Variable("y");
			
			Function function = Term.Sum
			(
				Enumerables.Create
				(
					Term.Product(Term.Constant(+5), Term.Product(x, x)),
					Term.Product(Term.Constant(-1), Term.Product(x, y)),
	   				Term.Product(Term.Constant(+1), Term.Product(y, y))
				)
			)
			.Abstract(x, y);
			
			Function rosenbrock = Term.Sum
			(
				Term.Difference(Term.Constant(1), x).Square(),
				Term.Product(Term.Constant(100), Term.Difference(y, x.Square()).Square())
			)
			.Abstract(x, y);
			
			Optimize(function, Enumerables.Create(1.0, 1.0));
			Optimize(rosenbrock, Enumerables.Create(-1.2, 1.0));
		}
		static void Optimize(Function function, IEnumerable<double> startPosition)
		{
			Variable x = new Variable("x");
			
			Rewriter simplifier = new Rewriter
			(
				Enumerables.Create<Rule>
				(
					new BetaReduction(),
					new EtaContraction(),
					new FirstOrderRule(Term.Sum(Term.Constant(0), x), x),
					new FirstOrderRule(Term.Sum(x, Term.Constant(0)), x),
					new FirstOrderRule(Term.Product(Term.Constant(0), x), Term.Constant(0)),
					new FirstOrderRule(Term.Product(x, Term.Constant(0)), Term.Constant(0)),
					new FirstOrderRule(Term.Product(Term.Constant(1), x), x),
					new FirstOrderRule(Term.Product(x, Term.Constant(1)), x),
					new FirstOrderRule(x.Exponentiate(0), Term.Constant(1)),
					new FirstOrderRule(x.Exponentiate(1), x)
				)
			);
			
			Problem problem = new Problem(function, Enumerable.Empty<Constraint>(), new Settings(), simplifier);
			IEnumerable<double> result = problem.Solve(startPosition);

			Console.WriteLine(function.GetText());
			Console.WriteLine("Start position: {0}", startPosition.ToStrings().Separate(", ").AggregateString());
			Console.WriteLine("Result position: {0}", result.ToStrings().Separate(", ").AggregateString());
		}
	}
}
