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
		readonly IEnumerable<ParametricCurve> segments;
		readonly IEnumerable<VirtualObject> virtualObjects;
		readonly Problem problem;
		
		// TODO
		// Cleanup Optimize method and output
		// Make stuff more efficient
		IEnumerable<Variable> Variables 
		{ 
			get 
			{ 
				return Enumerables.Concatenate
				(
					from segment in segments
					from parameter in segment.Parameters
					select parameter,
					from virtualObject in virtualObjects
					from variable in virtualObject.Variables
					select variable
				);
			} 
		}
		
		public Optimizer(IEnumerable<CurvePlaceSpecification> curvePlaceSpecifications, ParametricCurve segmentTemplate, int segmentCount)
		{
			if (curvePlaceSpecifications == null) throw new ArgumentNullException("curveSpecifications");
			if (segmentTemplate == null) throw new ArgumentNullException("segmentTemplate");
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");
			
			this.segments = 
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				let segmentParameters = 
					from parameterIndex in Enumerable.Range(0, segmentTemplate.Parameters.Count())
					select new Variable(string.Format("q_{0}_{1}", segmentIndex, parameterIndex))
				select segmentTemplate.RenameParameters(segmentParameters)
			)
			.ToArray();
			this.virtualObjects = Enumerables.Concatenate
			(
				CreateVirtualObjects
				(
					segments, 
					(index, attachmentPoints) => new VirtualPoint(index, attachmentPoints)
				), 
				CreateVirtualObjects
				(
					segments, 
					(index, attachmentPoints) => new VirtualVelocity(index, attachmentPoints)
				)
			)
			.ToArray(); 
			
			IEnumerable<CurveSpecification> curveSpecifications = CreateCurveSpecifications(curvePlaceSpecifications);
			
			Function objective = CreateObjective(curveSpecifications, segments, Variables.ToArray());
			
			Console.WriteLine("objective function\n{0}", objective);

			if (virtualObjects.Any()) 
			{
				CodomainConstrainedFunction constraints = CreateConstraints(virtualObjects, Variables.ToArray());
				
				Console.WriteLine("constraints function\n{0}", constraints.Function);

				this.problem = new Problem(objective, constraints, new Settings());
			}
			else 
			{
				this.problem = new Problem(objective, new Settings());
			}
		}

		public IEnumerable<ParametricCurve> Optimize()
		{
			Matrix startPosition = new Matrix(Variables.Count(), 1);
			
			Console.WriteLine("start position\n{0}", startPosition);

			Matrix resultPosition = problem.Solve(startPosition);
			
			Console.WriteLine("result position\n{0}", resultPosition);
			
			IEnumerable<double> result = resultPosition.Rows.Select(Enumerable.Single);
						
			int virtualObjectVariableCount = 
			(
				from virtualObject in virtualObjects
				from variable in virtualObject.Variables
				select variable
			)
			.Count();
			
			int parameterCount = segments.Select(parametricCurve => parametricCurve.Parameters.Count()).Distinct().Single();
			
			IEnumerable<ParametricCurve> resultCurves = 
			(
				from parametricCurveIndex in Enumerable.Range(0, segments.Count())
				let parametricCurve = segments.ElementAt(parametricCurveIndex)
				let parameterTerms = result.Skip(virtualObjectVariableCount).GetRange
				(
					(parametricCurveIndex + 0) * parameterCount, 
					(parametricCurveIndex + 1) * parameterCount
				)
				.Select(Term.Constant)
				select parametricCurve.InstantiateParameters(parameterTerms)
			)
			.ToArray();
			
			Console.WriteLine("virtual objects");
			foreach (double position in result.Take(virtualObjectVariableCount)) Console.WriteLine(position);

			Console.WriteLine("result curves");
			foreach (ParametricCurve curve in resultCurves) Console.WriteLine(curve);
			
			return resultCurves;
		}
		
		static IEnumerable<CurveSpecification> CreateCurveSpecifications(IEnumerable<CurvePlaceSpecification> curvePlaceSpecifications) 
		{
			return Enumerables.Concatenate<CurveSpecification>
			(
				from curvePlaceSpecification in curvePlaceSpecifications
				where curvePlaceSpecification.Point != null
				select new CurvePointSpecification(curvePlaceSpecification.Position, curvePlaceSpecification.Point.Item),
				from curvePlaceSpecification in curvePlaceSpecifications
				where curvePlaceSpecification.Velocity != null
				select new CurveVelocitySpecification(curvePlaceSpecification.Position, curvePlaceSpecification.Velocity.Item)
			);
		}
		static IEnumerable<VirtualObject> CreateVirtualObjects(IEnumerable<ParametricCurve> segmentCurves, Func<int, IEnumerable<CurvePoint>, VirtualObject> createVirtualObject) 
		{ 
			return 
				from segmentIndex in Enumerable.Range(0, segmentCurves.Count() - 1)
				select createVirtualObject
				(
					segmentIndex, 
					Enumerables.Create
					(
						new CurvePoint(segmentCurves.ElementAt(segmentIndex + 0), 1),
						new CurvePoint(segmentCurves.ElementAt(segmentIndex + 1), 0)
					)
				);
		}	
		static SymbolicFunction CreateObjective(IEnumerable<CurveSpecification> curveSpecifications, IEnumerable<ParametricCurve> segments, IEnumerable<Variable> variables)
		{
			return new SymbolicFunction
			(
				variables,
				Enumerables.Create
				(
					Term.Sum
					(
						from curveSpecification in curveSpecifications
						let segmentCount = segments.Count()
						let segmentLength = 1.0 / segmentCount
						let segmentIndex = (int)(curveSpecification.Position * segmentCount)
						let segmentPosition = (curveSpecification.Position - segmentLength * segmentIndex) / segmentLength
						let segmentPoint = 
							curveSpecification.Position == 1 ? 
							new CurvePoint(segments.Last(), 1) : 
				            new CurvePoint(segments.ElementAt(segmentIndex), segmentPosition)
						select curveSpecification.GetErrorTerm(segmentPoint)
					)
				)
			);
		}
		static CodomainConstrainedFunction CreateConstraints(IEnumerable<VirtualObject> virtualObjects, IEnumerable<Variable> variables) 
		{
			IEnumerable<Constraint> virtualObjectConstraints = 
			(
				from virtualObject in virtualObjects
				from constraint in virtualObject.Constraints
				select constraint
			)
			.ToArray();
			
			Function function = new SymbolicFunction(variables, virtualObjectConstraints.Select(constraint => constraint.Term));
			Range<Matrix> constraints = 
				new Range<Matrix>
				(
					Matrix.FromRowVectors(virtualObjectConstraints.Select(constraint => Matrix.CreateSingleton(constraint.Range.Start))),
					Matrix.FromRowVectors(virtualObjectConstraints.Select(constraint => Matrix.CreateSingleton(constraint.Range.End)))
				);
			
			return new CodomainConstrainedFunction(function, constraints);			
		}
	}
}

