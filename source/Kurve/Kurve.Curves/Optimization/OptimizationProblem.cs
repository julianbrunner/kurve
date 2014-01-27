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
			
			this.pointSpecificationTemplates =
			(
				from pointSpecificationIndex in Enumerable.Range(0, curveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification))
				select SpecificationTemplate.CreatePointSpecificationTemplate(optimizationSegments.Segments, pointSpecificationIndex)
			)
			.ToArray();
			this.directionSpecificationTemplates =
			(
				from directionSpecificationIndex in Enumerable.Range(0, curveSpecifications.Count(curveSpecification => curveSpecification is DirectionCurveSpecification))
				select SpecificationTemplate.CreateDirectionSpecificationTemplate(optimizationSegments.Segments, curveLength, directionSpecificationIndex)
			)
			.ToArray();
			this.curvatureSpecificationTemplates =
			(
				from curvatureSpecificationIndex in Enumerable.Range(0, curveSpecifications.Count(curveSpecification => curveSpecification is CurvatureCurveSpecification))
				select SpecificationTemplate.CreateCurvatureSpecificationTemplate(optimizationSegments.Segments, curveLength, curvatureSpecificationIndex)
			)
			.ToArray();

			IEnumerable<ValueTerm> variables = optimizationSegments.Parameters; 
			
			ValueTerm segmentLength = Terms.Quotient(curveLength, Terms.Constant(optimizationSegments.Segments.Count()));

			ValueTerm speedError = Terms.Sum
			(
				from segment in optimizationSegments.Segments
				let position = Terms.Variable("t")
				let curve = segment.LocalCurve
				let speed = curve.Speed.Apply(position)
				let error = Terms.Square(Terms.Difference(speed, segmentLength))
				select Terms.IntegrateTrapezoid(error.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);

			ValueTerm fairnessError = Terms.Sum
			(
				from segment in optimizationSegments.Segments
				let position = Terms.Variable("t")
				let curve = segment.LocalCurve
				let jerk = curve.Jerk.Apply(position)
				let normal = Terms.Normal(curve.Velocity.Apply(position))
				let error = Terms.Square(Terms.DotProduct(jerk, normal))
				select Terms.IntegrateTrapezoid(error.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);

//			ValueTerm pointSpecificationError = Terms.Sum
//			(
//				Enumerables.Concatenate
//				(
//					Enumerables.Create(Terms.Constant(0)),
//					from specificationTemplate in Enumerables.Concatenate(pointSpecificationTemplates)
//					select Terms.NormSquared
//					(
//						Terms.Difference
//						(
//							specificationTemplate.Constraint.Item,
//				 			Terms.Vector(specificationTemplate.Constraint.Ranges.Select(range => range.Start))
//						)
//					)
//				)
//			);
//			ValueTerm directionSpecificationError = Terms.Sum
//			(
//				Enumerables.Concatenate
//				(
//					Enumerables.Create(Terms.Constant(0)),
//					from specificationTemplate in Enumerables.Concatenate(directionSpecificationTemplates)
//					select Terms.NormSquared
//					(
//						Terms.Difference
//						(
//							specificationTemplate.Constraint.Item,
//				 			Terms.Vector(specificationTemplate.Constraint.Ranges.Select(range => range.Start))
//						)
//					)
//				)
//			);
//			ValueTerm curvatureSpecificationError = Terms.Sum
//			(
//				Enumerables.Concatenate
//				(
//					Enumerables.Create(Terms.Constant(0)),
//					from specificationTemplate in Enumerables.Concatenate(curvatureSpecificationTemplates)
//					select Terms.NormSquared
//					(
//						Terms.Difference
//						(
//							specificationTemplate.Constraint.Item,
//				 			Terms.Vector(specificationTemplate.Constraint.Ranges.Select(range => range.Start))
//						)
//					)
//				)
//			);

			ValueTerm objectiveValue = Terms.Sum
			(
				Terms.Product(Terms.Exponentiation(segmentLength, Terms.Constant(-2)),Terms.Constant(100000.0), speedError),
				Terms.Product(Terms.Exponentiation(segmentLength, Terms.Constant(-4)), Terms.Constant(1), fairnessError)
//				Terms.Product(Terms.Exponentiation(segmentLength, Terms.Constant(0)), Terms.Constant(0.1), pointSpecificationError),
//				Terms.Product(Terms.Exponentiation(segmentLength, Terms.Constant(1)), Terms.Constant(10), directionSpecificationError),
//				Terms.Product(Terms.Exponentiation(segmentLength, Terms.Constant(2)), Terms.Constant(100), curvatureSpecificationError)
			)
			.Simplify();

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

