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
			Problem problem = new Problem(new F());

			Matrix result = problem.Solve(Matrix.FromRowVectors(Enumerables.Create(1.0, 1.0).Select(Matrix.CreateSingleton)));

			Console.WriteLine(result);
		}
	}
	class F : Function
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
}
