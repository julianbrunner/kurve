using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves.Optimization
{
	public class OptimizationSubstitutions
	{
		readonly OptimizationProblem optimizationProblem;
		readonly double curveLength;
		readonly IEnumerable<CurveSpecification> curveSpecifications;

		readonly IEnumerable<Substitution> substitutions;

		public IEnumerable<Substitution> Substitutions { get { return substitutions; } }

		OptimizationSubstitutions(OptimizationProblem optimizationProblem, double curveLength, IEnumerable<CurveSpecification> curveSpecifications)
		{
			if (optimizationProblem == null) throw new ArgumentNullException("optimizationProblem");
			if (curveLength < 0) throw new ArgumentOutOfRangeException("curveLength");
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");

			this.optimizationProblem = optimizationProblem;
			this.curveLength = curveLength;
			this.curveSpecifications = curveSpecifications;

			this.substitutions = GetSubstitutions(optimizationProblem, curveLength, curveSpecifications).ToArray();
		}

		public bool NeedsRebuild(OptimizationProblem newOptimizationProblem, Specification newSpecification)
		{
			return
				optimizationProblem != newOptimizationProblem ||
				curveLength != newSpecification.BasicSpecification.CurveLength ||
				!Enumerable.SequenceEqual(curveSpecifications, newSpecification.BasicSpecification.CurveSpecifications);
		}

		public static OptimizationSubstitutions Create(OptimizationProblem optimizationProblem, Specification specification)
		{
			return new OptimizationSubstitutions
			(
				optimizationProblem,
				specification.BasicSpecification.CurveLength,
				specification.BasicSpecification.CurveSpecifications
			);
		}

		static IEnumerable<Substitution> GetSubstitutions(OptimizationProblem optimizationProblem, double curveLength, IEnumerable<CurveSpecification> curveSpecifications)
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
					foreach (int segmentIndex in Enumerable.Range(0, optimizationProblem.Segments.Count()))
					{
						Segment segment = optimizationProblem.Segments.ElementAt(segmentIndex);
						double localPosition = segment.PositionTransformation.Apply(Terms.Constant(pointCurveSpecification.Position)).Evaluate().Single();
						double segmentWeight = new OrderedRange<double>(0, 1).Contains(localPosition) ? 1 : 0;

						yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(segmentWeight));
					}
				}
				else
				{
					yield return new Substitution(position, Terms.Constant(0));
					yield return new Substitution(point, Terms.Constant(0, 0));
					foreach (int segmentIndex in Enumerable.Range(0, optimizationProblem.Segments.Count()))
						yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(0));
				}
			}
		}
	}
}

