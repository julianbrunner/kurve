using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves
{
	public static class Optimizer
	{
		public static IEnumerable<Curve> Optimize(Specification specification)
		{
			if (specification == null) throw new ArgumentNullException("specification");

			IEnumerable<Segment> segments =
			(
				from segmentIndex in Enumerable.Range(0, specification.SegmentCount)	
				select new Segment(specification.SegmentTemplate, GetPositionTransformation(segmentIndex, specification.SegmentCount), segmentIndex)
			)
			.ToArray();
			IEnumerable<ValueTerm> variables =
			(
				from segment in segments
				select segment.Parameter
			)
			.ToArray();
			
			NlpProblem problem = CreateProblem(specification.CurveSpecifications, specification.CurveLength / specification.SegmentCount, segments, variables);

			IEnumerable<double> result = problem.Solve(specification.Disambiguation);

			IEnumerable<Assignment> resultAssignments = Assignment.ValuesToAssignments(variables, result);

			IEnumerable<Curve> resultCurves =
			(
				from segment in segments
				let value = resultAssignments.Single(assignment => assignment.Variable == segment.Parameter).Value
				select segment.Instantiate(Terms.Constant(value))
			)
			.ToArray();

			Console.WriteLine("result curves");
			foreach (Curve curve in resultCurves) Console.WriteLine(curve);
			
			return resultCurves;
		}

		static NlpProblem CreateProblem(IEnumerable<CurveSpecification> curveSpecifications, double segmentLength, IEnumerable<Segment> segments, IEnumerable<ValueTerm> variables)
		{
			ValueTerm speedError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.GetLocalCurve()
				let segmentError = Terms.Square(Terms.Difference(Terms.Norm(segmentCurve.Velocity.Apply(position)), Terms.Constant(segmentLength)))
				select Terms.IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);
			ValueTerm fairnessError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.GetLocalCurve()
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

			IEnumerable<Constraint<ValueTerm>> constraintValues =
			(
				Enumerables.Concatenate
				(
					from curveSpecification in curveSpecifications
					from segment in GetAffectedSegments(segments, curveSpecification.Position)
					select Constraints.CreateZero(curveSpecification.GetErrorTerm(segment.GetGlobalCurve())),

					from segmentIndex in Enumerable.Range(0, segments.Count() - 1)
					let segment0CurvePoint = segments.ElementAt(segmentIndex + 0).GetLocalCurve().Point
					let segment1CurvePoint = segments.ElementAt(segmentIndex + 1).GetLocalCurve().Point
					select Constraints.CreateEquality
					(
						segment0CurvePoint.Apply(Terms.Constant(1)),
						segment1CurvePoint.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segments.Count() - 1)
					let segment0CurveVelocity = segments.ElementAt(segmentIndex + 0).GetLocalCurve().Velocity
					let segment1CurveVelocity = segments.ElementAt(segmentIndex + 1).GetLocalCurve().Velocity
					select Constraints.CreateEquality
					(
						segment0CurveVelocity.Apply(Terms.Constant(1)),
						segment1CurveVelocity.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segments.Count() - 1)
					let segment0CurveAcceleration = segments.ElementAt(segmentIndex + 0).GetLocalCurve().Acceleration
					let segment1CurveAcceleration = segments.ElementAt(segmentIndex + 1).GetLocalCurve().Acceleration
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

			FunctionTerm objectiveFunction = objectiveValue.Abstract(variables);
			Constraint<FunctionTerm> constraint = Constraints.Merge(constraintValues).Abstract(variables);

			return new NlpProblem(objectiveFunction, constraint, new Settings());
		}
		static FunctionTerm GetPositionTransformation(int segmentIndex, int segmentCount)
		{
			ValueTerm position = Terms.Variable("t");

			return Terms.Difference(Terms.Product(Terms.Constant(segmentCount), position), Terms.Constant(segmentIndex)).Abstract(position);
		}
		static IEnumerable<Segment> GetAffectedSegments(IEnumerable<Segment> segments, double position)
		{
			return
			(
				from segment in segments
				let localPosition = segment.PositionTransformation.Apply(Terms.Constant(position)).Evaluate().Single()
				where new OrderedRange<double>(0, 1).Contains(localPosition)
				select segment
			)
			.ToArray();
		}
	}
}

