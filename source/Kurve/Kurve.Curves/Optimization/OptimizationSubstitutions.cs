using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves.Optimization
{
	class OptimizationSubstitutions
	{
		readonly OptimizationSegments optimizationSegments;
		readonly OptimizationProblem optimizationProblem;
		readonly double curveLength;
		readonly IEnumerable<CurveSpecification> curveSpecifications;

		readonly IEnumerable<Substitution> substitutions;

		public IEnumerable<Substitution> Substitutions { get { return substitutions; } }

		OptimizationSubstitutions(OptimizationSegments optimizationSegments, OptimizationProblem optimizationProblem, double curveLength, IEnumerable<CurveSpecification> curveSpecifications)
		{
			if (optimizationSegments == null) throw new ArgumentNullException("optimizationSegments");
			if (optimizationProblem == null) throw new ArgumentNullException("optimizationProblem");
			if (curveLength < 0) throw new ArgumentOutOfRangeException("curveLength");
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");
			
			this.optimizationSegments = optimizationSegments;
			this.optimizationProblem = optimizationProblem;
			this.curveLength = curveLength;
			this.curveSpecifications = curveSpecifications;

			this.substitutions = GetSubstitutions(optimizationSegments, optimizationProblem, curveLength, curveSpecifications).ToArray();
		}

		public bool NeedsRebuild(OptimizationSegments newOptimizationSegments, OptimizationProblem newOptimizationProblem, Specification newSpecification)
		{
			return
				optimizationSegments != newOptimizationSegments ||
				optimizationProblem != newOptimizationProblem ||
				curveLength != newSpecification.BasicSpecification.CurveLength ||
				!Enumerable.SequenceEqual(curveSpecifications, newSpecification.BasicSpecification.CurveSpecifications);
		}

		public static OptimizationSubstitutions Create(OptimizationSegments optimizationSegments, OptimizationProblem optimizationProblem, Specification specification)
		{
			return new OptimizationSubstitutions
			(
				optimizationSegments,
				optimizationProblem,
				specification.BasicSpecification.CurveLength,
				specification.BasicSpecification.CurveSpecifications
			);
		}

		static IEnumerable<Substitution> GetSubstitutions(OptimizationSegments optimizationSegments, OptimizationProblem optimizationProblem, double curveLength, IEnumerable<CurveSpecification> curveSpecifications)
		{
			yield return new Substitution(optimizationProblem.CurveLength, Terms.Constant(curveLength));

			foreach (int pointSpecificationIndex in Enumerable.Range(0, optimizationProblem.PointSpecificationCount))
			{
				ValueTerm position = optimizationProblem.PointSpecificationPositions.ElementAt(pointSpecificationIndex);
				ValueTerm point = optimizationProblem.PointSpecificationPoints.ElementAt(pointSpecificationIndex);
				IEnumerable<ValueTerm> segmentWeights = optimizationProblem.PointSpecificationSegmentWeights.ElementAt(pointSpecificationIndex);

				if (pointSpecificationIndex < curveSpecifications.Count())
				{
					PointCurveSpecification pointCurveSpecification = (PointCurveSpecification)curveSpecifications.ElementAt(pointSpecificationIndex);
					
					yield return new Substitution(position, Terms.Constant(pointCurveSpecification.Position));
					yield return new Substitution(point, Terms.Constant(pointCurveSpecification.Point.X, pointCurveSpecification.Point.Y));
					foreach (int segmentIndex in Enumerable.Range(0, optimizationSegments.Segments.Count()))
					{
						Segment segment = optimizationSegments.Segments.ElementAt(segmentIndex);
						double segmentWeight = segment.Contains(pointCurveSpecification.Position) ? 1 : 0;

						yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(segmentWeight));
					}
				}
				else
				{
					yield return new Substitution(position, Terms.Constant(0));
					yield return new Substitution(point, Terms.Constant(0, 0));
					foreach (int segmentIndex in Enumerable.Range(0, optimizationSegments.Segments.Count()))
						yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(0));
				}
			}
		}
	}
}

