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
		
		IEnumerable<VirtualObject> VirtualPoints 
		{ 
			get 
			{ 
				return Enumerables.Concatenate
				(
					Enumerables.Create
					(
						new VirtualPoint
						(
							0, 
							Enumerables.Create(new VirtualObjectAttachmentSpecification(parametricCurves.First(), 0)),
							placeSpecifications.First().Point
						)
					),
					from segmentIndex in Enumerable.Range(0, parametricCurves.Count() - 1)
					select new VirtualPoint
					(
						segmentIndex + 1, 
						Enumerables.Create
						(
							new VirtualObjectAttachmentSpecification(parametricCurves.ElementAt(segmentIndex + 0), 1),
							new VirtualObjectAttachmentSpecification(parametricCurves.ElementAt(segmentIndex + 1), 0)
						),
						placeSpecifications.ElementAt(segmentIndex + 1).Point
					),
					Enumerables.Create
					(
						new VirtualPoint
						(
							placeSpecifications.Count() - 1, 
							Enumerables.Create(new VirtualObjectAttachmentSpecification(parametricCurves.Last(), 1)),
							placeSpecifications.Last().Point
						)
					)			
				);
			}
		}
		IEnumerable<VirtualObject> VirtualVelocities 
		{ 
			get 
			{ 
				return Enumerables.Concatenate
				(
					Enumerables.Create
					(
						new VirtualVelocity
						(
							0, 
							Enumerables.Create(new VirtualObjectAttachmentSpecification(parametricCurves.First(), 0)),
							placeSpecifications.First().Velocity
						)
					),
					from segmentIndex in Enumerable.Range(0, parametricCurves.Count() - 1)
					select new VirtualVelocity
					(
						segmentIndex + 1, 
						Enumerables.Create
						(
							new VirtualObjectAttachmentSpecification(parametricCurves.ElementAt(segmentIndex + 0), 1),
							new VirtualObjectAttachmentSpecification(parametricCurves.ElementAt(segmentIndex + 1), 0)
						),
						placeSpecifications.ElementAt(segmentIndex + 1).Velocity
					),
					Enumerables.Create
					(
						new VirtualVelocity
						(
							placeSpecifications.Count() - 1, 
							Enumerables.Create(new VirtualObjectAttachmentSpecification(parametricCurves.Last(), 1)),
							placeSpecifications.Last().Velocity
						)
					)			
				);
			}
		}
		IEnumerable<VirtualObject> VirtualObjects { get { return Enumerables.Concatenate(Enumerables.Create(VirtualPoints, VirtualVelocities)); } }
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
		IEnumerable<Variable> Variables 
		{ 
			get 
			{ 
				return Enumerables.Concatenate
				(
					from virtualObject in VirtualObjects
					from variable in virtualObject.Variables
					select variable,
					ParametricCurveParameters
				)
				.ToArray(); 
			} 
		}	
		
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
			
			this.objective = CreateObjective(Variables, VirtualObjects);
			this.constraints = CreateConstraints(Variables, VirtualObjects);
		}
		
		public IEnumerable<ParametricCurve> Optimize()
		{
			Problem problem = new Problem(objective, constraints, new Settings());
			
			Matrix startPosition = new Matrix(Variables.Count(), 1);
			Matrix resultPosition = problem.Solve(startPosition);

			IEnumerable<double> result = resultPosition.Rows.Select(Enumerable.Single);
						
			int virtualObjectVariableCount = 
			(
				from virtualObject in VirtualObjects
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
		
		static Function CreateObjective(IEnumerable<Variable> variables, IEnumerable<VirtualObject> virtualObjects) 
		{
			return new SymbolicFunction
			(
				variables,
				Enumerables.Create(Term.Sum(virtualObjects.Select(virtualObject => virtualObject.ErrorTerm)))
			);
		}	
		static CodomainConstrainedFunction CreateConstraints(IEnumerable<Variable> variables, IEnumerable<VirtualObject> virtualObjects) 
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

