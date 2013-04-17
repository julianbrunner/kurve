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
using Krach.Calculus.Terms.Basic.Atoms;
using System.Linq.Expressions;
using Krach.Calculus.Terms.Basic.Definitions;
using Krach.Calculus.Terms.Notation.Custom;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
        {
			Variable x = new Variable(1, "x");
			Variable y = new Variable(1, "y");

			// converges quickly
			FunctionTerm function1 = Term.Sum
			(
				Term.Product(Term.Constant(100), Term.Square(Term.Difference(x, Term.Constant(100)))),
				Term.Product(Term.Constant(1), Term.Square(Term.Difference(y, Term.Constant(1))))
			)
			.Abstract(x, y);

			// converges slowly, does weird things at the end, should also do exactly the same as function3
			FunctionTerm function2 = Term.Sum
			(
				Term.Product(Term.Constant(4)),
				Term.Product(Term.Constant(5), x),
				Term.Product(Term.Constant(1), y),
				Term.Product(Term.Constant(10), x, x),
				Term.Product(Term.Constant(-5), x, y),
				Term.Product(Term.Constant(1), y, y)
			)
			.Abstract(x, y);

			// converges slowly
			FunctionTerm function3 = Term.Sum
			(
				Term.Product(Term.Constant(4)),
				Term.Product(Term.Constant(5), x),
				Term.Product(Term.Constant(1), y),
				Term.Product(Term.Constant(10), x, x),
				Term.Product(Term.Constant(-5), y, x),
				Term.Product(Term.Constant(1), y, y)
			)
			.Abstract(x, y);

			// diverges
			FunctionTerm function4 = Term.Sum
			(
				Term.Product(Term.Constant(326700)),
				Term.Product(Term.Constant(100), Term.Square(Term.Difference(x, Term.Constant(100)))),
				Term.Product(Term.Constant(1), Term.Square(Term.Difference(y, Term.Constant(1)))),
				Term.Product(Term.Constant(10), Term.Product(Term.Difference(x, Term.Constant(1)), Term.Difference(y, Term.Constant(1))))
			)
			.Abstract(x, y);

			FunctionTerm rosenbrock = Term.Sum
			(
				Term.Square(Term.Difference(Term.Constant(1), x)),
				Term.Product(Term.Constant(100), Term.Square(Term.Difference(y, Term.Square(x))))
			)
			.Abstract(x, y);

			FunctionTerm f = new FunctionDefinition("f", Term.Identity(1), new BasicSyntax("f"));
			FunctionTerm g = new FunctionDefinition("g", Term.Identity(1), new BasicSyntax("g"));

			ValueTerm t = Term.Exponentiation(f.Apply(x), g.Apply(x)).GetDerivatives(x).Single();

			Console.WriteLine (Rewriting.CompleteSimplification.Rewrite(t));

			Optimize(function1, Enumerables.Create(0.0, 0.0));
			Optimize(function2, Enumerables.Create(0.0, 0.0));
			Optimize(function3, Enumerables.Create(0.0, 0.0));
			Optimize(function4, Enumerables.Create(0.0, 0.0));
			Optimize(rosenbrock, Enumerables.Create(-1.2, 1.0));

//			ValueTerm term = Term.Square(Term.Norm(Term.Polynomial(1, 4).Apply(Term.Vector(x, Term.Constant(1, 2, 3, 4)))));
//			term = Rewriting.CompleteNormalization.Rewrite(term).GetDerivatives(x).Single();
//
//			Console.WriteLine(term);
//			term = Rewriting.CompleteNormalization.RewriteAll(term);

//			Console.WriteLine(term);
//			Console.WriteLine(term.GetDerivatives(x).Single());
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
