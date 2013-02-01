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
		readonly IEnumerable<ParametricCurve> parametricCurves;
		readonly IEnumerable<VirtualObject> virtualObjects;
		readonly Function objective;
		readonly CodomainConstrainedFunction constraints;
		
		// TODO
		// Cleanup Optimize method and output
		// Make stuff more efficient
		IEnumerable<Variable> Variables 
		{ 
			get 
			{ 
				return Enumerables.Concatenate
				(
					from virtualObject in virtualObjects
					from variable in virtualObject.Variables
					select variable,
					from parametricCurve in parametricCurves
					from parameter in parametricCurve.Parameters
					select parameter
				);
			} 
		}
		
		public Optimizer(IEnumerable<CurvePlaceSpecification> placeSpecifications, ParametricCurve parametricCurveTemplate)
		{
			if (placeSpecifications == null) throw new ArgumentNullException("placeSpecifications");
			if (parametricCurveTemplate == null) throw new ArgumentNullException("uninstantiatedParametricCurve");
			
			this.parametricCurves = 
			(
				from segmentIndex in Enumerable.Range(0, placeSpecifications.Count() - 1)
				let segmentParameters = 
					from parameterIndex in Enumerable.Range(0, parametricCurveTemplate.Parameters.Count())
					select new Variable(string.Format("q_{0}_{1}", segmentIndex, parameterIndex))
				select parametricCurveTemplate.RenameParameters(segmentParameters)
			)
			.ToArray();
			this.virtualObjects = Enumerables.Concatenate
			(
				CreateVirtualObjects
				(
					placeSpecifications.Select(placeSpecification => placeSpecification.Point), 
					parametricCurves, 
					(index, specification, direction) => new VirtualPoint(index, specification, direction)
				), 
				CreateVirtualObjects
				(
					placeSpecifications.Select(placeSpecification => placeSpecification.Direction), 
					parametricCurves, 
					(index, specification, direction) => new VirtualDirection(index, specification, direction)
				)
			)
			.ToArray(); 
			
			this.objective = CreateObjective(virtualObjects, Variables.ToArray());
			this.constraints = CreateConstraints(virtualObjects, Variables.ToArray());
		}
		
		public IEnumerable<ParametricCurve> Optimize()
		{
			Problem problem = new Problem(objective, constraints, new Settings());
			
			Matrix startPosition = new Matrix(Variables.Count(), 1);
			Matrix resultPosition = problem.Solve(startPosition);

			IEnumerable<double> result = resultPosition.Rows.Select(Enumerable.Single);
						
			int virtualObjectVariableCount = 
			(
				from virtualObject in virtualObjects
				from variable in virtualObject.Variables
				select variable
			)
			.Count();
			
			int parameterCount = parametricCurves.Select(parametricCurve => parametricCurve.Parameters.Count()).Distinct().Single();
			
			IEnumerable<ParametricCurve> resultCurves = 
			(
				from parametricCurveIndex in Enumerable.Range(0, parametricCurves.Count())
				let parametricCurve = parametricCurves.ElementAt(parametricCurveIndex)
				let parameterTerms = result.Skip(virtualObjectVariableCount).GetRange
				(
					(parametricCurveIndex + 0) * parameterCount, 
					(parametricCurveIndex + 1) * parameterCount
				)
				.Select(Term.Constant)
				select parametricCurve.InstantiateParameters(parameterTerms)
			)
			.ToArray();

			Console.WriteLine("start position\n{0}", startPosition);
			Console.WriteLine("result position\n{0}", resultPosition);
			Console.WriteLine("objective function\n{0}", objective);
			Console.WriteLine("constraints function\n{0}", constraints.Function);

			Console.WriteLine("virtual objects");
			foreach (double position in result.Take(virtualObjectVariableCount)) Console.WriteLine(position);

			Console.WriteLine("result curves");
			foreach (ParametricCurve curve in resultCurves) Console.WriteLine(curve);
			
			return resultCurves;
		}
		
		static IEnumerable<VirtualObject> CreateVirtualObjects<T>(IEnumerable<T> virtualObjectSpecifications, IEnumerable<ParametricCurve> parametricCurves, Func<int, IEnumerable<VirtualObjectAttachmentSpecification>, T, VirtualObject> createVirtualObject) 
		{ 
			return Enumerables.Concatenate
			(
				Enumerables.Create
				(
					createVirtualObject
					(
						0, 
						Enumerables.Create(new VirtualObjectAttachmentSpecification(parametricCurves.First(), 0)),
						virtualObjectSpecifications.First()
					)
				),
				from segmentIndex in Enumerable.Range(0, parametricCurves.Count() - 1)
				select createVirtualObject
				(
					segmentIndex + 1, 
					Enumerables.Create
					(
						new VirtualObjectAttachmentSpecification(parametricCurves.ElementAt(segmentIndex + 0), 1),
						new VirtualObjectAttachmentSpecification(parametricCurves.ElementAt(segmentIndex + 1), 0)
					),
					virtualObjectSpecifications.ElementAt(segmentIndex + 1)
				),
				Enumerables.Create
				(
					createVirtualObject
					(
						virtualObjectSpecifications.Count() - 1, 
						Enumerables.Create(new VirtualObjectAttachmentSpecification(parametricCurves.Last(), 1)),
						virtualObjectSpecifications.Last()
					)
				)			
			);
		}
		static Function CreateObjective(IEnumerable<VirtualObject> virtualObjects, IEnumerable<Variable> variables) 
		{
			return new SymbolicFunction
			(
				variables,
				Enumerables.Create(Term.Sum(virtualObjects.Select(virtualObject => virtualObject.ErrorTerm)))
			);
		}	
		static CodomainConstrainedFunction CreateConstraints(IEnumerable<VirtualObject> virtualObjects, IEnumerable<Variable> variables) 
		{
			IEnumerable<Constraint> constraints = 
			(
				from virtualObject in virtualObjects
				from constraint in virtualObject.Constraints
				select constraint
			)
			.ToArray();
			
			return new CodomainConstrainedFunction
			(
				new SymbolicFunction(variables, constraints.Select(constraint => constraint.Term)),
				new Range<Matrix>
				(
					Matrix.FromRowVectors(constraints.Select(constraint => Matrix.CreateSingleton(constraint.Range.Start))),
					Matrix.FromRowVectors(constraints.Select(constraint => Matrix.CreateSingleton(constraint.Range.End)))
				)
			);			
		}
	}
}

