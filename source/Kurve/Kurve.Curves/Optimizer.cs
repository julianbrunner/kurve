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
		readonly IEnumerable<CurvePlaceSpecification> placeSpecifications;
		readonly IEnumerable<ParametricCurve> parametricCurves;
		readonly Function objective;
		readonly CodomainConstrainedFunction constraints;
		
		IEnumerable<Variable> VirtualPoints 
		{ 
			get 
			{ 
				return 
					from virtualPointIndex in Enumerable.Range(0, placeSpecifications.Count())
					from componentIndex in Enumerables.Create(0, 1)
					select new Variable(string.Format("p_{0}_{1}", virtualPointIndex, componentIndex));
			}
		}
		IEnumerable<Variable> ParametricCurveParameters 
		{ 
			get
			{
				return
					from parametricCurve in parametricCurves
					from parameter in parametricCurve.Parameters
					select parameter;
			}
		}
		IEnumerable<Variable> Variables { get { return Enumerables.Concatenate(VirtualPoints, ParametricCurveParameters).ToArray(); } }	
		
		public Optimizer(IEnumerable<CurvePlaceSpecification> placeSpecifications, ParametricCurve parametricCurveTemplate)
		{
			if (placeSpecifications == null) throw new ArgumentNullException("placeSpecifications");
			if (parametricCurveTemplate == null) throw new ArgumentNullException("uninstantiatedParametricCurve");
			
			this.placeSpecifications = placeSpecifications;
			this.parametricCurves = 
			(
				from segmentIndex in Enumerable.Range(0, placeSpecifications.Count() - 1)
				let segmentParameters = 
					from parameterIndex in Enumerable.Range(0, parametricCurveTemplate.Parameters.Count())
					select new Variable(string.Format("q_{0}_{1}", segmentIndex, parameterIndex))
				select parametricCurveTemplate.RenameParameters(segmentParameters)
			)
			.ToArray();
			
			this.objective = new SymbolicFunction
			(
				Variables,
				Enumerables.Create
				(
					Term.Sum
					(
						from item in Enumerable.Zip(Enumerable.Range(0, placeSpecifications.Count()), placeSpecifications, Tuple.Create)
						let virtualPointX = Term.Variable(string.Format("p_{0}_0", item.Item1))
						let virtualPointY = Term.Variable(string.Format("p_{0}_1", item.Item1))
						let placeSpecification = item.Item2
						select Term.Sum
						(
							Term.Difference(virtualPointX, Term.Constant(placeSpecification.Position.X)).Square(),
							Term.Difference(virtualPointY, Term.Constant(placeSpecification.Position.Y)).Square()
						)
					)
				)
			);
			
			Function constraintsFunction = new SymbolicFunction
			(
				Variables,
				from parametricCurveIndex in Enumerable.Range(0, parametricCurves.Count())
				let parametricCurve1 = parametricCurves.ElementAt(parametricCurveIndex)
				from segmentPositionIndex in Enumerables.Create(0, 1)
				let parametricCurve2 = parametricCurve1.InstantiatePosition(Term.Constant(segmentPositionIndex))
				let parametricCurveTerms = Enumerables.Create(parametricCurve2.X, parametricCurve2.Y)
				let virtualPointIndex = ((parametricCurveIndex * 2 + segmentPositionIndex) + 1) / 2
				from componentIndex in Enumerables.Create(0, 1)
				let virtualPoint = Term.Variable(string.Format("p_{0}_{1}", virtualPointIndex, componentIndex))
				select Term.Difference(parametricCurveTerms.ElementAt(componentIndex), virtualPoint)
			);
			
			Range<Matrix> constraintsConstraints = new Range<Matrix>(new Matrix(parametricCurves.Count() * 2 * 2, 1));
			
			this.constraints = new CodomainConstrainedFunction(constraintsFunction, constraintsConstraints);
			// TODO: next steps
			//   think about how object encapsulation can be used to hide some of the index wars
		}
		
		public IEnumerable<ParametricCurve> Optimize()
		{
			Problem problem = new Problem(objective, constraints, new Settings());
			
			Matrix startPosition = new Matrix(Variables.Count(), 1);
			IEnumerable<double> result = problem.Solve(startPosition).Rows.Select(Enumerable.Single);
						
			int virtualPointsCount = VirtualPoints.Count();
			int parameterCount = parametricCurves.Select(parametricCurve => parametricCurve.Parameters.Count()).Distinct().Single();
			
			IEnumerable<ParametricCurve> resultCurves = 
			(
				from parametricCurveIndex in Enumerable.Range(0, parametricCurves.Count())
				let parametricCurve = parametricCurves.ElementAt(parametricCurveIndex)
				let parameterTerms = result.Skip(virtualPointsCount).GetRange
				(
					(parametricCurveIndex + 0) * parameterCount, 
					(parametricCurveIndex + 1) * parameterCount
				)
				.Select(Term.Constant)
				select parametricCurve.InstantiateParameters(parameterTerms)
			)	
			.ToArray();
			
			Console.WriteLine(objective);
			Console.WriteLine(constraints.Function);
			
			foreach (double position in result.Take(virtualPointsCount)) Console.WriteLine(position);
			foreach (ParametricCurve curve in resultCurves) Console.WriteLine(curve);
			
			return resultCurves;
		}
	}
}

