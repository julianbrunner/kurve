using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves.Optimization
{
	public class Optimizer
	{
		OptimizationProblem optimizationProblem;
		OptimizationSubstitutions optimizationSubstitutions;
		OptimizationSolver optimizationSolver;
		OptimizationPosition optimizationPosition;

		public Optimizer()
		{
			Rebuild(new Specification());
		}

		public Specification Normalize(Specification specification)
		{
			Rebuild(specification);

			return new Specification(specification.BasicSpecification, optimizationPosition.Position);
		}
		public IEnumerable<Curve> GetCurves(Specification specification)
		{
			Rebuild(specification);

			IEnumerable<Assignment> resultAssignments = Assignment.ValuesToAssignments(optimizationProblem.Variables, optimizationPosition.Position);

			return
			(
				from segment in optimizationProblem.Segments
				let value = resultAssignments.Single(assignment => assignment.Variable == segment.Parameter).Value
				select segment.InstantiateLocalCurve(Terms.Constant(value))
			)
			.ToArray();
		}

		void Rebuild(Specification specification)
		{
			if (optimizationProblem == null || optimizationProblem.NeedsRebuild(specification))
			{
				Console.WriteLine("Rebuilding OptimizationProblem...");
				optimizationProblem = OptimizationProblem.Create(specification);
			}
			if (optimizationSubstitutions == null || optimizationSubstitutions.NeedsRebuild(optimizationProblem, specification))
			{
				Console.WriteLine("Rebuilding OptimizationSubstitutions...");
				optimizationSubstitutions = OptimizationSubstitutions.Create(optimizationProblem, specification);
			}
			if (optimizationSolver == null || optimizationSolver.NeedsRebuild(optimizationProblem, optimizationSubstitutions))
			{
				Console.WriteLine("Rebuilding OptimizationSolver...");
				optimizationSolver = OptimizationSolver.Create(optimizationProblem, optimizationSubstitutions);
			}
			if (optimizationPosition == null || optimizationPosition.NeedsRebuild(optimizationSolver, specification.Position))
			{
				Console.WriteLine("Rebuilding OptimizationPosition...");
				optimizationPosition = OptimizationPosition.Create(optimizationSolver, specification.Position);
			}
		}
	}
}

