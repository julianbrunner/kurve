using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves.Optimization
{
	class OptimizationProblem
	{
		readonly OptimizationSegments optimizationSegments;
		readonly IEnumerable<CurveSpecification> curveSpecifications;

		readonly ValueTerm curveLength;
		readonly IEnumerable<SpecificationTemplate> pointSpecificationTemplates;
		readonly IEnumerable<SpecificationTemplate> directionSpecificationTemplates;
		readonly IEnumerable<SpecificationTemplate> curvatureSpecificationTemplates;
		readonly IpoptProblem problem;

		public ValueTerm CurveLength { get { return curveLength; } }
		public IEnumerable<SpecificationTemplate> PointSpecificationTemplates { get { return pointSpecificationTemplates; } }
		public IEnumerable<SpecificationTemplate> DirectionSpecificationTemplates { get { return directionSpecificationTemplates; } }
		public IEnumerable<SpecificationTemplate> CurvatureSpecificationTemplates { get { return curvatureSpecificationTemplates; } }
		public IpoptProblem Problem { get { return problem; } }

		OptimizationProblem(OptimizationSegments optimizationSegments, IEnumerable<CurveSpecification> curveSpecifications)
		{
			if (optimizationSegments == null) throw new ArgumentNullException("segmentManager");
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");

			this.optimizationSegments = optimizationSegments;
			this.curveSpecifications = curveSpecifications;

			this.curveLength = Terms.Variable("curveLength");

			IEnumerable<ValueTerm> variables = optimizationSegments.Parameters; 

			ValueTerm velocityError = Terms.Sum
			(
				from segment in optimizationSegments.Segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.LocalCurve
				let segmentLength = Terms.Quotient(curveLength, Terms.Constant(optimizationSegments.Segments.Count()))
				let segmentError = Terms.Square(Terms.Difference(segmentCurve.Speed.Apply(position), segmentLength))
				select Terms.IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);
			ValueTerm fairnessError = Terms.Sum
			(
				from segment in optimizationSegments.Segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.LocalCurve
				let segmentError = Terms.Square(Terms.Norm(segmentCurve.Acceleration.Apply(position)))
				select Terms.IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);

			ValueTerm objectiveValue = Terms.Sum
			(
				Terms.Product(Terms.Constant(1.0), velocityError),
				Terms.Product(Terms.Constant(0.0001), fairnessError)
			)
			.Simplify();


			this.pointSpecificationTemplates =
			(
				from pointSpecificationIndex in Enumerable.Range(0, curveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification))
				select SpecificationTemplate.CreatePointSpecificationTemplate(optimizationSegments.Segments, pointSpecificationIndex)
			)
			.ToArray();
			this.directionSpecificationTemplates =
			(
				from directionSpecificationIndex in Enumerable.Range(0, curveSpecifications.Count(curveSpecification => curveSpecification is DirectionCurveSpecification))
				select SpecificationTemplate.CreateDirectionSpecificationTemplate(optimizationSegments.Segments, directionSpecificationIndex)
			)
			.ToArray();
			this.curvatureSpecificationTemplates =
			(
				from curvatureSpecificationIndex in Enumerable.Range(0, curveSpecifications.Count(curveSpecification => curveSpecification is CurvatureCurveSpecification))
				select SpecificationTemplate.CreateCurvatureSpecificationTemplate(optimizationSegments.Segments, curvatureSpecificationIndex)
			)
			.ToArray();

			IEnumerable<Constraint<ValueTerm>> constraintValues =
			(
				Enumerables.Concatenate
				(
					from pointSpecificationTemplate in pointSpecificationTemplates
					select pointSpecificationTemplate.Constraint,
					from directionSpecificationTemplate in directionSpecificationTemplates
					select directionSpecificationTemplate.Constraint,
					from curvatureSpecificationTemplate in curvatureSpecificationTemplates
					select curvatureSpecificationTemplate.Constraint,

					from segmentIndex in Enumerable.Range(0, optimizationSegments.Segments.Count() - 1)
					let segment0CurvePoint = optimizationSegments.Segments.ElementAt(segmentIndex + 0).LocalCurve.Point
					let segment1CurvePoint = optimizationSegments.Segments.ElementAt(segmentIndex + 1).LocalCurve.Point
					select Constraints.CreateEquality
					(
						segment0CurvePoint.Apply(Terms.Constant(1)),
						segment1CurvePoint.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, optimizationSegments.Segments.Count() - 1)
					let segment0CurveVelocity = optimizationSegments.Segments.ElementAt(segmentIndex + 0).LocalCurve.Velocity
					let segment1CurveVelocity = optimizationSegments.Segments.ElementAt(segmentIndex + 1).LocalCurve.Velocity
					select Constraints.CreateEquality
					(
						segment0CurveVelocity.Apply(Terms.Constant(1)),
						segment1CurveVelocity.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, optimizationSegments.Segments.Count() - 1)
					let segment0CurveAcceleration = optimizationSegments.Segments.ElementAt(segmentIndex + 0).LocalCurve.Acceleration
					let segment1CurveAcceleration = optimizationSegments.Segments.ElementAt(segmentIndex + 1).LocalCurve.Acceleration
					select Constraints.CreateEquality
					(
						segment0CurveAcceleration.Apply(Terms.Constant(1)),
						segment1CurveAcceleration.Apply(Terms.Constant(0))
					)
				)
			)
			.ToArray();

			Constraint<ValueTerm> constraint = Constraints.Merge(constraintValues);

			FunctionTerm objectiveFunction = objectiveValue.Abstract(variables);
			FunctionTerm constraintFunction = constraint.Item.Abstract(variables);

			this.problem = IpoptProblem.Create(objectiveFunction, constraintFunction, constraint.Ranges);
		}

		public bool NeedsRebuild(OptimizationSegments newOptimizationSegments, Specification newSpecification)
		{
			return
				optimizationSegments != newOptimizationSegments ||
				curveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification) < newSpecification.BasicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification) ||
				curveSpecifications.Count(curveSpecification => curveSpecification is DirectionCurveSpecification) < newSpecification.BasicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is DirectionCurveSpecification) ||
				curveSpecifications.Count(curveSpecification => curveSpecification is CurvatureCurveSpecification) < newSpecification.BasicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is CurvatureCurveSpecification);
		}

		public static OptimizationProblem Create(OptimizationSegments optimizationSegments, Specification specification)
		{
			return new OptimizationProblem
			(
				optimizationSegments,
				specification.BasicSpecification.CurveSpecifications
			);
		}
	}
}

