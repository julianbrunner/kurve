using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves.Optimization
{
	public class OptimizationPosition
	{
		readonly OptimizationSolver optimizationSolver;
		readonly IEnumerable<double> initialPosition;

		readonly IEnumerable<double> position;

		public IEnumerable<double> Position { get { return position; } }

		OptimizationPosition(OptimizationSolver optimizationSolver, IEnumerable<double> initialPosition)
		{
			if (optimizationSolver == null) throw new ArgumentNullException("optimizationSolver");
			if (initialPosition == null) throw new ArgumentNullException("initialPosition");

			this.optimizationSolver = optimizationSolver;
			this.initialPosition = initialPosition;

			this.position = optimizationSolver.Solver.Solve(initialPosition);
		}

		public bool NeedsRebuild(OptimizationSolver newOptimizationSolver, IEnumerable<double> newInitialPosition)
		{
			return
				optimizationSolver != newOptimizationSolver ||
				!Enumerable.SequenceEqual(initialPosition, newInitialPosition) && !Enumerable.SequenceEqual(position, newInitialPosition);
		}

		public static OptimizationPosition Create(OptimizationSolver optimizationSolver, IEnumerable<double> initialPosition)
		{
			return new OptimizationPosition(optimizationSolver, initialPosition);
		}
	}
}

