using System;
using System.Linq;
using Kurve.Ipopt;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Krach.Calculus;
using Krach.Calculus.Terms;

namespace Kurve.Curves
{
	public class Optimizer
	{
//		class ConstraintsFunction : Function
//		{
//			readonly Optimizer optimizer;
//
//			public override int DomainDimension { get { return optimizer.DomainDimension; } }
//			public override int CodomainDimension { get { return (optimizer.placeSpecifications.Count() - 1) * 4; } }
//
//			public ConstraintsFunction(Optimizer optimizer)
//			{
//				if (optimizer == null) throw new ArgumentNullException("optimizer");
//
//				this.optimizer = optimizer;
//			}
//
//			public override IEnumerable<Matrix> GetValues(Matrix position)
//			{
//				IEnumerable<Vector2Double> virtualPoints = optimizer.GetVirtualPoints(position);
//				IEnumerable<ParametricCurve> parametricCurves = optimizer.GetParametricCurves(position);
//
//				return
//					from parametricCurveIndex in Enumerable.Range(0, parametricCurves.Count())
//					let parametricCurve = parametricCurves.ElementAt(parametricCurveIndex)
//					from segmentPositionIndex in Enumerables.Create(0, 1)
//					let listIndex = parametricCurveIndex * 2 + segmentPositionIndex
//					let virtualPointIndex = (listIndex + 1) / 2
//					let point = parametricCurve.EvaluatePoint(segmentPositionIndex) - virtualPoints.ElementAt(virtualPointIndex)
//					from component in Enumerables.Create(point.X, point.Y)
//					select Matrix.CreateSingleton(component);
//			}
//			public override IEnumerable<Matrix> GetGradients(Matrix position)
//			{
//				throw new System.NotImplementedException();
//			}
//			public override IEnumerable<Matrix> GetHessians(Matrix position)
//			{
//				throw new System.NotImplementedException();
//			}
//		}

		readonly IEnumerable<CurvePlaceSpecification> placeSpecifications;
		readonly ParametricCurve parametricCurveTemplate;
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
					(placeSpecifications.Count() - 1) * parametricCurveTemplate.Parameters.Count();
			}
		}

		public Optimizer(IEnumerable<CurvePlaceSpecification> placeSpecifications, ParametricCurve parametricCurveTemplate)
		{
			if (placeSpecifications == null) throw new ArgumentNullException("placeSpecifications");
			if (parametricCurveTemplate == null) throw new ArgumentNullException("uninstantiatedParametricCurve");

			this.placeSpecifications = placeSpecifications;
			this.parametricCurveTemplate = parametricCurveTemplate;

			IEnumerable<Variable> virtualPoints =
				from virtualPointIndex in Enumerable.Range(0, placeSpecifications.Count())
				from component in Enumerables.Create("x", "y")
				select new Variable(string.Format("p_{0}_{1}", virtualPointIndex, component));
			IEnumerable<Variable> parameters =
				from segmentIndex in Enumerable.Range(0, placeSpecifications.Count() - 1)
				from parameterIndex in Enumerable.Range(0, parametricCurveTemplate.Parameters.Count())
				from component in Enumerables.Create("x", "y")
				select new Variable(string.Format("q_{0}_{1}_{2}", segmentIndex, parameterIndex, component));

			IEnumerable<Variable> variables = Enumerables.Concatenate(virtualPoints, parameters).ToArray();

			this.fairnessFunction = new SymbolicFunction
			(
				variables,
				Enumerables.Create
				(
					Term.Sum
					(
						from item in Enumerable.Zip(Enumerable.Range(0, placeSpecifications.Count()), placeSpecifications, Tuple.Create)
						let virtualPointX = Term.Variable(string.Format("p_{0}_x", item.Item1))
						let virtualPointY = Term.Variable(string.Format("p_{0}_y", item.Item1))
						let placeSpecification = item.Item2
						select Term.Sum
						(
							Term.Difference(virtualPointX, Term.Constant(placeSpecification.Position.X)).Square(),
							Term.Difference(virtualPointY, Term.Constant(placeSpecification.Position.Y)).Square()
						)
					)
				)
			);
			this.constraintsFunction = new SymbolicFunction
			(
				variables,
				null
				// TODO: insert copies of curve template according to notes
			);

			// TODO: next steps
			//   think about how object encapsulation can be used to hide some of the index wars

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
//				select Matrix.CreateSingleton(component)
//			);
		}
	}
}

