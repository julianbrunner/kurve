using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using System.Linq.Expressions;
using Wrappers.Casadi;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
        {
			ValueTerm x = Terms.Variable("x");
			ValueTerm y = Terms.Variable("y");

//			FunctionTerm test1 = Terms.Vector(x.Select(0), y, Terms.Constant(42)).Abstract(y, x);
//			FunctionTerm test2 = Terms.Sum(Terms.Sum(Terms.Constant(1), y), Terms.Constant(2)).Abstract(y);
//			ValueTerm test3 = Terms.DotProduct(Terms.Constant(2, 1, 4), Terms.Vector(Terms.Constant(1, 2), x));
//			ValueTerm test4 = Terms.Sum(x, y).Abstract(x).Apply(Terms.Constant(3));
//			FunctionTerm test5 = Terms.Constant(5).Abstract(x, y).Apply(y, y).Abstract(y);

			// converges quickly
			FunctionTerm function1 = Terms.Sum
			(
				Terms.Product(Terms.Constant(100), Terms.Square(Terms.Difference(x, Terms.Constant(100)))),
				Terms.Product(Terms.Constant(1), Terms.Square(Terms.Difference(y, Terms.Constant(1))))
			)
			.Abstract(x, y);

			// converges slowly, does weird things at the end, should also do exactly the same as function3
			FunctionTerm function2 = Terms.Sum
			(
				Terms.Product(Terms.Constant(4)),
				Terms.Product(Terms.Constant(5), x),
				Terms.Product(Terms.Constant(1), y),
				Terms.Product(Terms.Constant(10), x, x),
				Terms.Product(Terms.Constant(-5), x, y),
				Terms.Product(Terms.Constant(1), y, y)
			)
			.Abstract(x, y);

			// converges slowly
			FunctionTerm function3 = Terms.Sum
			(
				Terms.Product(Terms.Constant(4)),
				Terms.Product(Terms.Constant(5), x),
				Terms.Product(Terms.Constant(1), y),
				Terms.Product(Terms.Constant(10), x, x),
				Terms.Product(Terms.Constant(-5), y, x),
				Terms.Product(Terms.Constant(1), y, y)
			)
			.Abstract(x, y);

			// diverges
			FunctionTerm function4 = Terms.Sum
			(
				Terms.Product(Terms.Constant(326700)),
				Terms.Product(Terms.Constant(100), Terms.Square(Terms.Difference(x, Terms.Constant(100)))),
				Terms.Product(Terms.Constant(1), Terms.Square(Terms.Difference(y, Terms.Constant(1)))),
				Terms.Product(Terms.Constant(10), Terms.Product(Terms.Difference(x, Terms.Constant(1)), Terms.Difference(y, Terms.Constant(1))))
			)
			.Abstract(x, y);

			FunctionTerm rosenbrock = Terms.Sum
			(
				Terms.Square(Terms.Difference(Terms.Constant(1), x)),
				Terms.Product(Terms.Constant(100), Terms.Square(Terms.Difference(y, Terms.Square(x))))
			)
			.Abstract(x, y);

			Console.WriteLine(function1);
			Console.WriteLine(function2);
			Console.WriteLine(function3);
			Console.WriteLine(function4);
			Console.WriteLine(rosenbrock);

//			FunctionTerm test = rosenbrock;
//			Console.WriteLine (test);
//			Console.WriteLine (test.DomainDimension);
//			Console.WriteLine (test.CodomainDimension);

//			Optimize(function1, Enumerables.Create(1.0, 1.0));
//			Optimize(function2, Enumerables.Create(1.0, 1.0));
//			Optimize(function3, Enumerables.Create(1.0, 1.0));
//			Optimize(function4, Enumerables.Create(1.0, 1.0));
			Optimize(rosenbrock, Enumerables.Create(-1.2, 1.0));
		}
		static void WriteDerivatives(FunctionTerm function, int depth)
		{
			function = function.Simplify();

			Console.WriteLine(new string(' ', 4 - 2 * depth) + function);

			if (depth > 0)
				foreach (FunctionTerm derivative in function.GetDerivatives())
					WriteDerivatives(derivative, depth - 1);
		}
		static void Optimize(FunctionTerm objectiveFunction, IEnumerable<double> startPosition)
		{
			ValueTerm position = Terms.Variable("x", objectiveFunction.DomainDimension);
			Constraint<FunctionTerm> constraint = Constraints.Merge().Abstract(position);

			NlpProblem problem = new NlpProblem(objectiveFunction, constraint, new Settings());
			
			IEnumerable<double> resultPosition = problem.Solve(startPosition);

			Console.WriteLine("function: {0}", objectiveFunction);
			Console.WriteLine("start position: {0}", startPosition.ToStrings().Separate(", ").AggregateString());
			Console.WriteLine("result position: {0}", resultPosition.ToStrings().Separate(", ").AggregateString());
		}
	}
}
