using System;
using Kurve.Ipopt;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
		{
			Console.WriteLine(Optimize(new QuadradicFunction(), Matrix.FromRowVectors(Enumerables.Create(1.0, 1.0).Select(Matrix.CreateSingleton))));
			Console.WriteLine(Optimize(new Rosenbrock(), Matrix.FromRowVectors(Enumerables.Create(-1.2, 1.0).Select(Matrix.CreateSingleton))));
		}
		static Matrix Optimize(Function function, Matrix startPosition)
		{
			Problem problem = new Problem(function, new Settings { PrintLevel = 4, MaximumIterationCount = 9001 });

			return problem.Solve(startPosition);
		}
	}
	class QuadradicFunction : Function
	{
		readonly Matrix A = new Matrix
		(
			new double[,]
			{
				{ 1, 0 },
				{ 0, 2 }
			}
		);

		public override int DomainDimension { get { return 2; } }
		public override int CodomainDimension { get { return 1; } }

		public override IEnumerable<Matrix> GetValues(Matrix position)
		{
			yield return position.Transpose * A * position;
		}
		public override IEnumerable<Matrix> GetGradients(Matrix position)
		{
			yield return (A + A.Transpose) * position;
		}
		public override IEnumerable<Matrix> GetHessians(Matrix position)
		{
			yield return A + A.Transpose;
		}
	}
	class Rosenbrock : Function
	{
		public override int DomainDimension { get { return 2; } }
		public override int CodomainDimension { get { return 1; } }

		public override IEnumerable<Matrix> GetValues(Matrix position)
		{
			yield return new Matrix
			(
				new [,]
				{
					{ (1 - position[0, 0]).Square() + 100 * (position[1, 0] - position[0, 0].Square()).Square() }
				}
			);
		}
		public override IEnumerable<Matrix> GetGradients(Matrix position)
		{
			yield return new Matrix
			(
				new [,]
				{
					{ 2 * (200 * position[0, 0].Exponentiate(3) - 200 * position[0, 0] * position[1, 0] + position[0, 0] - 1) },
					{ 200 * (position[1, 0] - position[0, 0].Square()) }
				}
			);
		}
		public override IEnumerable<Matrix> GetHessians(Matrix position)
		{
			yield return new Matrix
			(
				new [,]
				{
					{ 1200 * position[0, 0].Square() - 400 * position[1, 0] + 2, - 400 * position[0, 0] },
					{ - 400 * position[0, 0], 200 }
				}
			);
		}
	}
}
