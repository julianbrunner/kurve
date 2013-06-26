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
		readonly int pointSpecificationCount;
		readonly IEnumerable<ValueTerm> pointSpecificationPositions;
		readonly IEnumerable<ValueTerm> pointSpecificationPoints;
		readonly IEnumerable<IEnumerable<ValueTerm>> pointSpecificationSegmentWeights;
		readonly IpoptProblem problem;

		public ValueTerm CurveLength { get { return curveLength; } }
		public int PointSpecificationCount { get { return pointSpecificationCount; } }
		public IEnumerable<ValueTerm> PointSpecificationPositions { get { return pointSpecificationPositions; } }
		public IEnumerable<ValueTerm> PointSpecificationPoints { get { return pointSpecificationPoints; } }
		public IEnumerable<IEnumerable<ValueTerm>> PointSpecificationSegmentWeights { get { return pointSpecificationSegmentWeights; } }
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
				let segmentError = Terms.Square(Terms.Difference(Terms.Norm(segmentCurve.Velocity.Apply(position)), segmentLength))
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

//			Console.WriteLine("objective value");
//			Console.WriteLine(objectiveValue);
//			Console.WriteLine();

			this.pointSpecificationCount = curveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification);
			this.pointSpecificationPositions =
			(
				from pointSpecificationIndex in Enumerable.Range(0, pointSpecificationCount)
				select Terms.Variable(string.Format("position_{0}", pointSpecificationIndex))
			)
			.ToArray();
			this.pointSpecificationPoints =
			(
				from pointSpecificationIndex in Enumerable.Range(0, pointSpecificationCount)
				select Terms.Variable(string.Format("point_{0}", pointSpecificationIndex), 2)
			)
			.ToArray();
			this.pointSpecificationSegmentWeights =
			(
				from pointSpecificationIndex in Enumerable.Range(0, pointSpecificationCount)
				select
				(
					from segmentIndex in Enumerable.Range(0, optimizationSegments.Segments.Count())
					select Terms.Variable(string.Format("point_{0}_segment_weight_{1}", pointSpecificationIndex, segmentIndex))
				)
				.ToArray()
			)
			.ToArray();

			IEnumerable<Constraint<ValueTerm>> constraintValues =
			(
				Enumerables.Concatenate
				(
					from pointSpecificationIndex in Enumerable.Range(0, pointSpecificationCount)
					let position = pointSpecificationPositions.ElementAt(pointSpecificationIndex)
					let point = pointSpecificationPoints.ElementAt(pointSpecificationIndex)
					let segmentWeights = pointSpecificationSegmentWeights.ElementAt(pointSpecificationIndex)
					select Constraints.CreateZero
					(
						Terms.Sum
						(
							from segmentIndex in Enumerable.Range(0, optimizationSegments.Segments.Count())
							let segment = optimizationSegments.Segments.ElementAt(segmentIndex)
							let segmentWeight = segmentWeights.ElementAt(segmentIndex)
							select Terms.Scaling(segmentWeight, PointCurveSpecification.GetErrorTerm(segment.GlobalCurve, position, point))
						)
					),

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

//			Console.WriteLine("constraint values");
//			foreach (Constraint<ValueTerm> constraintValue in constraintValues) Console.WriteLine(constraintValue);
//			Console.WriteLine();

			Constraint<ValueTerm> constraint = Constraints.Merge(constraintValues);

			FunctionTerm objectiveFunction = objectiveValue.Abstract(variables);
			FunctionTerm constraintFunction = constraint.Item.Abstract(variables);

			this.problem = IpoptProblem.Create(objectiveFunction, constraintFunction, constraint.Ranges);
		}

		public bool NeedsRebuild(OptimizationSegments newOptimizationSegments, Specification newSpecification)
		{
			return
				optimizationSegments != newOptimizationSegments ||
				curveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification) < newSpecification.BasicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification);
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

