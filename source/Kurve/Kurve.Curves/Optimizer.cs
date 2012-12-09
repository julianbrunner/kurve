using System;
using System.Linq;
using Kurve.Ipopt;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;

namespace Kurve.Curves
{
	public class Optimizer
	{
		class FairnessFunction : Function
		{
			readonly Optimizer optimizer;

			public override int DomainDimension { get { return optimizer.DomainDimension; } }
			public override int CodomainDimension { get { return 1; } }

			public FairnessFunction(Optimizer optimizer)
			{
				if (optimizer == null) throw new ArgumentNullException("optimizer");

				this.optimizer = optimizer;
			}

			public override IEnumerable<Matrix> GetValues(Matrix position)
			{
				IEnumerable<Vector2Double> virtualPoints =
					from index in Enumerable.Range(0, optimizer.placeSpecifications.Count())
					let x = position[index * 2 + 0, 0]
					let y = position[index * 2 + 1, 0]
					select new Vector2Double(x, y);

				double result = Enumerable.Zip
				(
					virtualPoints,
					optimizer.placeSpecifications,
					(virtualPoint, placeSpecification) => (virtualPoint - placeSpecification.Position).LengthSquared
				)
				.Sum();

				yield return Matrix.CreateSingleton(result);
			}
			public override IEnumerable<Matrix> GetGradients(Matrix position)
			{
				IEnumerable<Vector2Double> virtualPoints =
					from index in Enumerable.Range(0, optimizer.placeSpecifications.Count())
					let x = position[index * 2 + 0, 0]
					let y = position[index * 2 + 1, 0]
					select new Vector2Double(x, y);

				IEnumerable<double> result1 =
					from point in Enumerable.Zip
					(
						virtualPoints,
						optimizer.placeSpecifications,
						(virtualPoint, placeSpecification) => 2 * (virtualPoint - placeSpecification.Position)
					)
					from component in Enumerables.Create(point.X, point.Y)
					select component;
				IEnumerable<double> result2 = Enumerable.Repeat(0.0, (optimizer.placeSpecifications.Count() - 1) * optimizer.classSpecification.ParameterCount);

				yield return Matrix.FromRowVectors(Enumerables.Concatenate(result1, result2).Select(Matrix.CreateSingleton));
			}
			public override IEnumerable<Matrix> GetHessians(Matrix position)
			{
				Matrix result = new Matrix(optimizer.DomainDimension, optimizer.DomainDimension);

				for (int index = 0; index < optimizer.placeSpecifications.Count() * 2; index++) result[index, index] = 2;

				return result;
			}
		}
		class ConstraintsFunction : Function
		{
			public override int DomainDimension
			{
				get
				{
					throw new System.NotImplementedException();
				}
			}
			public override int CodomainDimension
			{
				get
				{
					throw new System.NotImplementedException();
				}
			}

			public override IEnumerable<Matrix> GetValues(Matrix position)
			{
				throw new System.NotImplementedException();
			}
			public override IEnumerable<Matrix> GetGradients(Matrix position)
			{
				throw new System.NotImplementedException();
			}
			public override IEnumerable<Matrix> GetHessians(Matrix position)
			{
				throw new System.NotImplementedException();
			}
		}

		readonly IEnumerable<CurvePlaceSpecification> placeSpecifications;
		readonly ParametricCurveClassSpecification classSpecification;

		int DomainDimension
		{
			get
			{
				return
					// virtual points
					placeSpecifications.Count() * 2 +
					// parametric curve parameters
					(placeSpecifications.Count() - 1) * classSpecification.ParameterCount;
			}
		}

		public Optimizer(IEnumerable<CurvePlaceSpecification> placeSpecifications, ParametricCurveClassSpecification classSpecification)
		{
			if (placeSpecifications == null) throw new ArgumentNullException("placeSpecifications");
			if (classSpecification == null) throw new ArgumentNullException("classSpecification");

			this.placeSpecifications = placeSpecifications;
			this.classSpecification = classSpecification;
		}
	}
}

