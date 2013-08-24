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

			IEnumerable<PointCurveSpecification> pointCurveSpecifications = curveSpecifications.OfType<PointCurveSpecification>();
			IEnumerable<DirectionCurveSpecification> directionCurveSpecifications = curveSpecifications.OfType<DirectionCurveSpecification>();
			IEnumerable<CurvatureCurveSpecification> curvatureCurveSpecifications = curveSpecifications.OfType<CurvatureCurveSpecification>();

			foreach (int pointSpecificationTemplateIndex in Enumerable.Range(0, optimizationProblem.PointSpecificationTemplates.Count()))
			{
				SpecificationTemplate pointSpecificationTemplate = optimizationProblem.PointSpecificationTemplates.ElementAt(pointSpecificationTemplateIndex);

				if (pointSpecificationTemplateIndex < pointCurveSpecifications.Count())
				{
					PointCurveSpecification pointCurveSpecification = pointCurveSpecifications.ElementAt(pointSpecificationTemplateIndex);

					ValueTerm position = Terms.Constant(pointCurveSpecification.Position);
					ValueTerm value = Terms.Constant(pointCurveSpecification.Point.X, pointCurveSpecification.Point.Y);

					foreach (Substitution substitution in pointSpecificationTemplate.GetSubstitutions(optimizationSegments.Segments, pointCurveSpecification.Position, position, value))
						yield return substitution;
				}
				else
					foreach (Substitution substitution in pointSpecificationTemplate.GetSubstitutions(optimizationSegments.Segments))
						yield return substitution;
			}

			foreach (int directionSpecificationTemplateIndex in Enumerable.Range(0, optimizationProblem.DirectionSpecificationTemplates.Count()))
			{
				SpecificationTemplate directionSpecificationTemplate = optimizationProblem.DirectionSpecificationTemplates.ElementAt(directionSpecificationTemplateIndex);

				if (directionSpecificationTemplateIndex < directionCurveSpecifications.Count())
				{
					DirectionCurveSpecification directionCurveSpecification = directionCurveSpecifications.ElementAt(directionSpecificationTemplateIndex);

					ValueTerm position = Terms.Constant(directionCurveSpecification.Position);
					ValueTerm value = Terms.Direction(Terms.Constant(directionCurveSpecification.Direction));

					foreach (Substitution substitution in directionSpecificationTemplate.GetSubstitutions(optimizationSegments.Segments, directionCurveSpecification.Position, position, value))
						yield return substitution;
				}
				else
					foreach (Substitution substitution in directionSpecificationTemplate.GetSubstitutions(optimizationSegments.Segments))
						yield return substitution;
			}

			foreach (int curvatureSpecificationTemplateIndex in Enumerable.Range(0, optimizationProblem.CurvatureSpecificationTemplates.Count()))
			{
				SpecificationTemplate curvatureSpecificationTemplate = optimizationProblem.CurvatureSpecificationTemplates.ElementAt(curvatureSpecificationTemplateIndex);

				if (curvatureSpecificationTemplateIndex < curvatureCurveSpecifications.Count())
				{
					CurvatureCurveSpecification curvatureCurveSpecification = curvatureCurveSpecifications.ElementAt(curvatureSpecificationTemplateIndex);

					ValueTerm position = Terms.Constant(curvatureCurveSpecification.Position);
					ValueTerm value = Terms.Constant(curvatureCurveSpecification.Curvature);

					foreach (Substitution substitution in curvatureSpecificationTemplate.GetSubstitutions(optimizationSegments.Segments, curvatureCurveSpecification.Position, position, value))
						yield return substitution;
				}
				else
					foreach (Substitution substitution in curvatureSpecificationTemplate.GetSubstitutions(optimizationSegments.Segments))
						yield return substitution;
			}
		}
	}
}

