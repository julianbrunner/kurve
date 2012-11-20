using System;
using Kurve.Ipopt;
using Krach.Basics;
using System.Collections.Generic;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
		{

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
			yield return 0.5 * (position.Transpose * A + A * position);
		}
		public override IEnumerable<Matrix> GetHessians(Matrix position)
		{
			yield return 0.5 * (A + A.Transpose);
		}
	}
}
