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
		readonly int segmentCount;
		readonly FunctionTermCurveTemplate segmentTemplate;
		readonly IEnumerable<CurveSpecification> curveSpecifications;

		readonly IEnumerable<Segment> segments;
		readonly ValueTerm curveLength;
		readonly int pointSpecificationCount;
		readonly IEnumerable<ValueTerm> pointSpecificationPositions;
		readonly IEnumerable<ValueTerm> pointSpecificationPoints;
		readonly IEnumerable<IEnumerable<ValueTerm>> pointSpecificationSegmentWeights;
		readonly IpoptProblem problem;

		public IEnumerable<Segment> Segments { get { return segments; } }
		public ValueTerm CurveLength { get { return curveLength; } }
		public int PointSpecificationCount { get { return pointSpecificationCount; } }
		public IEnumerable<ValueTerm> PointSpecificationPositions { get { return pointSpecificationPositions; } }
		public IEnumerable<ValueTerm> PointSpecificationPoints { get { return pointSpecificationPoints; } }
		public IEnumerable<IEnumerable<ValueTerm>> PointSpecificationSegmentWeights { get { return pointSpecificationSegmentWeights; } }
		public IpoptProblem Problem { get { return problem; } }

		OptimizationProblem(int segmentCount, FunctionTermCurveTemplate segmentTemplate, IEnumerable<CurveSpecification> curveSpecifications)
		{
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");
			if (segmentTemplate == null) throw new ArgumentNullException("segmentTemplate");
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");

			this.segmentCount = segmentCount;
			this.segmentTemplate = segmentTemplate;
			this.curveSpecifications = curveSpecifications;

			IEnumerable<ValueTerm> parameters =
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				select Terms.Variable(string.Format("sp_{0}", segmentIndex), segmentTemplate.ParameterDimension)
			)
			.ToArray();

			this.segments =
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				let parameter = parameters.ElementAt(segmentIndex)
				let curve = segmentTemplate.InstantiateParameter(parameter)
				let position = Terms.Variable("t")
				let positionTransformation = Terms.Difference(Terms.Product(Terms.Constant(segmentCount), position), Terms.Constant(segmentIndex)).Abstract(position)
				select new Segment(curve, positionTransformation)
			)
			.ToArray();

			this.curveLength = Terms.Variable("curveLength");

			IEnumerable<ValueTerm> variables = parameters; 

			ValueTerm speedError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.LocalCurve
				let segmentLength = Terms.Quotient(curveLength, Terms.Constant(segmentCount))
				let segmentError = Terms.Square(Terms.Difference(Terms.Norm(segmentCurve.Velocity.Apply(position)), segmentLength))
				select Terms.IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);
			ValueTerm fairnessError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.LocalCurve
				let segmentError = Terms.Square(Terms.Norm(segmentCurve.Acceleration.Apply(position)))
				select Terms.IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);

			ValueTerm objectiveValue = Terms.Sum
			(
				Terms.Product(Terms.Constant(1.0), speedError),
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
					from segmentIndex in Enumerable.Range(0, segments.Count())
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
							from segmentIndex in Enumerable.Range(0, segments.Count())
							let segment = segments.ElementAt(segmentIndex)
							let segmentWeight = segmentWeights.ElementAt(segmentIndex)
							select Terms.Scaling(segmentWeight, PointCurveSpecification.GetErrorTerm(segment.GlobalCurve, position, point))
						)
					),

					from segmentIndex in Enumerable.Range(0, segments.Count() - 1)
					let segment0CurvePoint = segments.ElementAt(segmentIndex + 0).LocalCurve.Point
					let segment1CurvePoint = segments.ElementAt(segmentIndex + 1).LocalCurve.Point
					select Constraints.CreateEquality
					(
						segment0CurvePoint.Apply(Terms.Constant(1)),
						segment1CurvePoint.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segments.Count() - 1)
					let segment0CurveVelocity = segments.ElementAt(segmentIndex + 0).LocalCurve.Velocity
					let segment1CurveVelocity = segments.ElementAt(segmentIndex + 1).LocalCurve.Velocity
					select Constraints.CreateEquality
					(
						segment0CurveVelocity.Apply(Terms.Constant(1)),
						segment1CurveVelocity.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segments.Count() - 1)
					let segment0CurveAcceleration = segments.ElementAt(segmentIndex + 0).LocalCurve.Acceleration
					let segment1CurveAcceleration = segments.ElementAt(segmentIndex + 1).LocalCurve.Acceleration
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

		public bool NeedsRebuild(Specification newSpecification)
		{
			return
				segmentCount != newSpecification.BasicSpecification.SegmentCount ||
				segmentTemplate != newSpecification.BasicSpecification.SegmentTemplate ||
				curveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification) < newSpecification.BasicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification);
		}

		public static OptimizationProblem Create(Specification specification)
		{
			return new OptimizationProblem
			(
				specification.BasicSpecification.SegmentCount,
				specification.BasicSpecification.SegmentTemplate,
				specification.BasicSpecification.CurveSpecifications
			);
		}
	}
}

