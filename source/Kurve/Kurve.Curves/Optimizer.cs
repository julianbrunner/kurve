using System;
using System.Linq;
using Kurve.Ipopt;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Krach.Calculus;

namespace Kurve.Curves
{
	public class Optimizer
	{
		class ConstraintsFunction : Function
		{
			readonly Optimizer optimizer;

			public override int DomainDimension { get { return optimizer.DomainDimension; } }
			public override int CodomainDimension { get { return (optimizer.placeSpecifications.Count() - 1) * 4; } }

			public ConstraintsFunction(Optimizer optimizer)
			{
				if (optimizer == null) throw new ArgumentNullException("optimizer");

				this.optimizer = optimizer;
			}

			public override IEnumerable<Matrix> GetValues(Matrix position)
			{
				IEnumerable<Vector2Double> virtualPoints = optimizer.GetVirtualPoints(position);
				IEnumerable<ParametricCurve> parametricCurves = optimizer.GetParametricCurves(position);

				return
					from parametricCurveIndex in Enumerable.Range(0, parametricCurves.Count())
					let parametricCurve = parametricCurves.ElementAt(parametricCurveIndex)
					from segmentPositionIndex in Enumerables.Create(0, 1)
					let listIndex = parametricCurveIndex * 2 + segmentPositionIndex
					let virtualPointIndex = (listIndex + 1) / 2
					let point = parametricCurve.EvaluatePoint(segmentPositionIndex) - virtualPoints.ElementAt(virtualPointIndex)
					from component in Enumerables.Create(point.X, point.Y)
					select Matrix.CreateSingleton(component);
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
		readonly Function fairnessFunction;
		readonly Function constraintsFunction;

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

			IEnumerable<Variable> virtualPoints =
				from virtualPointIndex in Enumerable.Range(0, placeSpecifications.Count())
				from component in Enumerables.Create("x", "y")
				select new Variable(string.Format("p_{0}_{1}", virtualPointIndex, component));
			IEnumerable<Variable> parameters =
				from segmentIndex in Enumerable.Range(0, placeSpecifications.Count() - 1)
				from parameterIndex in Enumerable.Range(0, classSpecification.ParameterCount)
				from component in Enumerables.Create("x", "y")
				select new Variable(string.Format("q_{0}_{1}_{2}", segmentIndex, parameterIndex, component));

			IEnumerable<Variable> variables = Enumerables.Concatenate(virtualPoints, parameters).ToArray();

			this.fairnessFunction = new SymbolicFunction
			(
				variables,
				Enumerables.Create
				(
					(
						from item in Enumerable.Zip(Enumerable.Range(0, placeSpecifications.Count()), placeSpecifications, Tuple.Create)
						let virtualPoint = string.Format("p_{0}", item.Item1)
						let placeSpecification = item.Item2
						select Terms.Sum
						(
							Terms.Difference(Terms.Variable(virtualPoint + "_x"), Terms.Constant(placeSpecification.Position.X)).Square(),
							Terms.Difference(Terms.Variable(virtualPoint + "_y"), Terms.Constant(placeSpecification.Position.Y)).Square()
						)
					)
					.Sum()
				)
			);

			// TODO: next steps
			//   generalize ParametricCurve to use terms with parameter and position variables
			//   make objects for things like curve segments

//			this.constraintsFunction = new SymbolicFunction
//			(
//				variables,
//				from segmentIndex in Enumerable.Range(0, placeSpecifications.Count() - 1)
//				let parametricCurve = parametricCurves.ElementAt(segmentIndex)
//				from segmentPositionIndex in Enumerables.Create(0, 1)
//				let listIndex = parametricCurveIndex * 2 + segmentPositionIndex
//				let virtualPointIndex = (listIndex + 1) / 2
//				let point = parametricCurve.EvaluatePoint(segmentPositionIndex) - virtualPoints.ElementAt(virtualPointIndex)
//				from component in Enumerables.Create(point.X, point.Y)
//				select Matrix.CreateSingleton(component);
//			)
		}

		IEnumerable<Vector2Double> GetVirtualPoints(Matrix position)
		{
			int offset = 0;

			return
				from pointIndex in Enumerable.Range(0, placeSpecifications.Count())
				let x = position[offset + pointIndex * 2 + 0, 0]
				let y = position[offset + pointIndex * 2 + 1, 0]
				select new Vector2Double(x, y);
		}
		IEnumerable<ParametricCurve> GetParametricCurves(Matrix position)
		{
			int offset = placeSpecifications.Count() * 2;

			return
				from segmentIndex in Enumerable.Range(0, placeSpecifications.Count() - 1)
				let parameters =
					from parameterIndex in Enumerable.Range(0, classSpecification.ParameterCount)
					select position[offset + segmentIndex * classSpecification.ParameterCount + parameterIndex, 0]
				select classSpecification.CreateCurve(parameters);
		}
	}
}

