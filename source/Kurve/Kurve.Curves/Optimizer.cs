using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves
{
	public class Optimizer
	{
		readonly BasicSpecification basicSpecification;
		readonly OptimizationProblem optimizationProblem;
		readonly IEnumerable<double> position;

		Optimizer(BasicSpecification basicSpecification, OptimizationProblem optimizationProblem, IEnumerable<double> position)
		{
			if (basicSpecification == null) throw new ArgumentNullException("basicSpecification");
			if (optimizationProblem == null) throw new ArgumentNullException("optimizationProblem");
			if (position == null) throw new ArgumentNullException("position");

			this.basicSpecification = basicSpecification;
			this.optimizationProblem = optimizationProblem;
			this.position = position;
		}

		public Optimizer Modify(BasicSpecification newBasicSpecification)
		{
			OptimizationProblem newOptimizationProblem = optimizationProblem.AdjustSize(newBasicSpecification);
			
			IEnumerable<double> startPosition;
			if (newBasicSpecification.SegmentCount == basicSpecification.SegmentCount && newBasicSpecification.SegmentTemplate == basicSpecification.SegmentTemplate) startPosition = position;
			else startPosition = newBasicSpecification.GetDefaultPosition();

			IEnumerable<double> newPosition = newOptimizationProblem.GetProblem(newBasicSpecification).Solve(startPosition);

			return new Optimizer(newBasicSpecification, newOptimizationProblem, newPosition);
		}
		public IEnumerable<Curve> GetCurves()
		{
			return optimizationProblem.GetCurves(position);
		}

		public static Optimizer Create(BasicSpecification newBasicSpecification)
		{
			OptimizationProblem newOptimizationProblem = OptimizationProblem.Create(newBasicSpecification);
			
			IEnumerable<double> startPosition = newBasicSpecification.GetDefaultPosition();

			IEnumerable<double> newPosition = newOptimizationProblem.GetProblem(newBasicSpecification).Solve(startPosition);

			return new Optimizer(newBasicSpecification, newOptimizationProblem, newPosition);
		}
	}
}

