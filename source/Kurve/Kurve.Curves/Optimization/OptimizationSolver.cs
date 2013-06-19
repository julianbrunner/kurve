using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves.Optimization
{
	class OptimizationSolver
	{
		readonly OptimizationProblem optimizationProblem;
		readonly OptimizationSubstitutions optimizationSubstitutions;

		readonly IpoptSolver solver;

		public IpoptSolver Solver { get { return solver; } }

		OptimizationSolver(OptimizationProblem optimizationProblem, OptimizationSubstitutions optimizationSubstitutions)
		{
			if (optimizationProblem == null) throw new ArgumentNullException("optimizationProblem");
			if (optimizationSubstitutions == null) throw new ArgumentNullException("optimizationSubstitutions");

			this.optimizationProblem = optimizationProblem;
			this.optimizationSubstitutions = optimizationSubstitutions;

			this.solver = new IpoptSolver(optimizationProblem.Problem.Substitute(optimizationSubstitutions.Substitutions), new Settings());
		}

		public bool NeedsRebuild(OptimizationProblem newOptimizationProblem, OptimizationSubstitutions newOptimizationSubstitutions)
		{
			return
				optimizationProblem != newOptimizationProblem ||
				optimizationSubstitutions != newOptimizationSubstitutions;
		}

		public static OptimizationSolver Create(OptimizationProblem optimizationProblem, OptimizationSubstitutions optimizationSubstitutions)
		{
			return new OptimizationSolver(optimizationProblem, optimizationSubstitutions);
		}
	}
}

