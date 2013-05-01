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
		readonly IEnumerable<Segment> segments;
		readonly NlpProblem problem;

		public IEnumerable<ValueTerm> Variables
		{
			get
			{
				return from segment in segments select segment.Parameter;
			}
		}
		
		public Optimizer(IEnumerable<PositionedCurveSpecification> curveSpecifications, CurveTemplate segmentCurveTemplate, int segmentCount, double curveLength)
		{
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");
			if (segmentCurveTemplate == null) throw new ArgumentNullException("segmentCurve");
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");
			if (curveLength < 0) throw new ArgumentOutOfRangeException("curveLength");

			this.segments =
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)	
				select new Segment(segmentCurveTemplate, GetPositionTransformation(segmentIndex, segmentCount), segmentIndex)
			)
			.ToArray();
			
			double segmentLength = curveLength / segmentCount;

			ValueTerm speedError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.GetLocalCurve()
				let segmentError = Terms.Square(Terms.Difference(Terms.Norm(segmentCurve.Velocity.Apply(position)), Terms.Constant(segmentLength)))
				select IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 10)
			);
			ValueTerm fairnessError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.GetLocalCurve()
				let segmentError = Terms.Square(Terms.Norm(segmentCurve.Acceleration.Apply(position)))
				select IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 10)
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

					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
					let segment0CurvePoint = segments.ElementAt(segmentIndex + 0).GetLocalCurve().Point
					let segment1CurvePoint = segments.ElementAt(segmentIndex + 1).GetLocalCurve().Point
					select Constraints.CreateEquality
					(
						segment0CurvePoint.Apply(Terms.Constant(1)),
						segment1CurvePoint.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
					let segment0CurveVelocity = segments.ElementAt(segmentIndex + 0).GetLocalCurve().Velocity
					let segment1CurveVelocity = segments.ElementAt(segmentIndex + 1).GetLocalCurve().Velocity
					select Constraints.CreateEquality
					(
						segment0CurveVelocity.Apply(Terms.Constant(1)),
						segment1CurveVelocity.Apply(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
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

			FunctionTerm objectiveFunction = objectiveValue.Abstract(Variables);
			Constraint<FunctionTerm> constraint = Constraints.Merge(constraintValues).Abstract(Variables);

			this.problem = new NlpProblem(objectiveFunction, constraint, new Settings());
		}

		public IEnumerable<Curve> Optimize()
		{
			IEnumerable<double> result = problem.Solve
			(
				from variable in Variables
				from component in Enumerable.Range(0, variable.Dimension)
				select new OrderedRange<double>(-10, +10),
				10
			);

			IEnumerable<Assignment> resultAssignments = Assignment.ValuesToAssignments(Variables, result);

			Console.WriteLine("result assignments");
			foreach (Assignment assignment in resultAssignments) Console.WriteLine(assignment);

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

		static ValueTerm IntegrateTrapezoid(FunctionTerm function, OrderedRange<double> bounds, int segmentCount)
		{
			if (segmentCount < 1) throw new ArgumentOutOfRangeException("segmentCount");

			ValueTerm segmentWidth = Terms.Constant(bounds.Length() / segmentCount);

			IEnumerable<ValueTerm> values =
			(
				from segmentPosition in Scalars.GetIntermediateValues(bounds.Start, bounds.End, segmentCount)
				select function.Apply(Terms.Constant(segmentPosition))
			)
			.ToArray();

			return Terms.Product
			(
				segmentWidth,
				Terms.Sum
				(
					Enumerables.Concatenate
					(
						Enumerables.Create(Terms.Product(Terms.Constant(0.5), values.First())),
						values.Skip(1).SkipLast(1),
						Enumerables.Create(Terms.Product(Terms.Constant(0.5), values.Last()))
					)
				)
			);
		}
	}
}

