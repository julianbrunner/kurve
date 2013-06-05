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
		readonly IEnumerable<Segment> segments;
		readonly IEnumerable<ValueTerm> variables;
		readonly IEnumerable<double> position;

		Optimizer(BasicSpecification basicSpecification, IEnumerable<Segment> segments, IEnumerable<ValueTerm> variables, IEnumerable<double> position)
		{
			if (basicSpecification == null) throw new ArgumentNullException("basicSpecification");
			if (segments == null) throw new ArgumentNullException("segments");
			if (variables == null) throw new ArgumentNullException("variables");
			if (position == null) throw new ArgumentNullException("position");

			this.basicSpecification = basicSpecification;
			this.segments = segments;
			this.variables = variables;
			this.position = position;
		}

		public Optimizer Modify(BasicSpecification newBasicSpecification)
		{
			IEnumerable<Segment> newSegments =
			(
				from segmentIndex in Enumerable.Range(0, newBasicSpecification.SegmentCount)	
				select new Segment(newBasicSpecification.SegmentTemplate, GetPositionTransformation(segmentIndex, newBasicSpecification.SegmentCount), segmentIndex)
			)
			.ToArray();

			IEnumerable<ValueTerm> newVariables =
			(
				from segment in newSegments
				select segment.Parameter
			)
			.ToArray();

			NlpProblem newNlpProblem = GetNlpProblem(newBasicSpecification, newSegments, newVariables);
			
			IEnumerable<double> startPosition;

			if (newBasicSpecification.SegmentCount == basicSpecification.SegmentCount && newBasicSpecification.SegmentTemplate == basicSpecification.SegmentTemplate) startPosition = position;
			else startPosition = newBasicSpecification.GetDefaultPosition();

			IEnumerable<double> newPosition = newNlpProblem.Solve(startPosition);

			return new Optimizer(newBasicSpecification, newSegments, newVariables, newPosition);
		}
		public IEnumerable<Curve> GetCurves()
		{
			IEnumerable<Assignment> resultAssignments = Assignment.ValuesToAssignments(variables, position);

			return
			(
				from segment in segments
				let value = resultAssignments.Single(assignment => assignment.Variable == segment.Parameter).Value
				select segment.InstantiateLocalCurve(Terms.Constant(value))
			)
			.ToArray();
		}

		public static Optimizer Create(BasicSpecification newBasicSpecification)
		{
			IEnumerable<Segment> newSegments =
			(
				from segmentIndex in Enumerable.Range(0, newBasicSpecification.SegmentCount)	
				select new Segment(newBasicSpecification.SegmentTemplate, GetPositionTransformation(segmentIndex, newBasicSpecification.SegmentCount), segmentIndex)
			)
			.ToArray();

			IEnumerable<ValueTerm> newVariables =
			(
				from segment in newSegments
				select segment.Parameter
			)
			.ToArray();

			NlpProblem newNlpProblem = GetNlpProblem(newBasicSpecification, newSegments, newVariables);
			
			IEnumerable<double> startPosition = newBasicSpecification.GetDefaultPosition();

			IEnumerable<double> newPosition = newNlpProblem.Solve(startPosition);

			return new Optimizer(newBasicSpecification, newSegments, newVariables, newPosition);
		}

		static NlpProblem GetNlpProblem(BasicSpecification basicSpecification, IEnumerable<Segment> segments, IEnumerable<ValueTerm> variables)
		{
			ValueTerm curveLength = Terms.Variable("curveLength");

			ValueTerm speedError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.GetLocalCurve()
				let segmentLength = Terms.Quotient(curveLength, Terms.Constant(basicSpecification.SegmentCount))
				let segmentError = Terms.Square(Terms.Difference(Terms.Norm(segmentCurve.Velocity.Apply(position)), segmentLength))
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
					from curveSpecification in basicSpecification.CurveSpecifications
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

			Constraint<ValueTerm> constraint = Constraints.Merge(constraintValues);

			FunctionTerm objectiveFunction = objectiveValue.Abstract(variables);
			FunctionTerm constraintFunction = constraint.Item.Abstract(variables);

			IpoptProblem problem = IpoptProblem.Create(objectiveFunction, constraintFunction);

			problem = problem.Substitute(curveLength, Terms.Constant(basicSpecification.CurveLength));

			return new NlpProblem(problem, constraint.Ranges, new Settings());
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

