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
		OptimizationSegments optimizationSegments;
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
		public Curve GetCurve(Specification specification)
		{
			Rebuild(specification);

			return optimizationSegments.GetCurve(optimizationPosition.Position);
		}

		void Rebuild(Specification specification)
		{
			if (optimizationSegments == null || optimizationSegments.NeedsRebuild(specification))
			{
				Console.WriteLine("Rebuilding OptimizationSegments...");
				optimizationSegments = OptimizationSegments.Create(specification);
			}
			if (optimizationProblem == null || optimizationProblem.NeedsRebuild(optimizationSegments, specification))
			{
				Console.WriteLine("Rebuilding OptimizationProblem...");
				optimizationProblem = OptimizationProblem.Create(optimizationSegments, specification);
			}
			if (optimizationSubstitutions == null || optimizationSubstitutions.NeedsRebuild(optimizationSegments, optimizationProblem, specification))
			{
				Console.WriteLine("Rebuilding OptimizationSubstitutions...");
				optimizationSubstitutions = OptimizationSubstitutions.Create(optimizationSegments, optimizationProblem, specification);
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

