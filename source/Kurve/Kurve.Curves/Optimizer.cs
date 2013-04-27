using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;

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

			ValueTerm velocityLengthError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentVelocity = segment.GetLocalCurve().Derivative.InstantiatePosition(position)
				let segmentVelocityLengthError = Terms.Square(Terms.Difference(Terms.Norm(segmentVelocity), Terms.Constant(segmentLength)))
				select IntegrateTrapezoid(segmentVelocityLengthError.Abstract(position), new OrderedRange<double>(0, 1), 100)
			);

			ValueTerm objectiveValue = Terms.Sum
			(
				Terms.Product(Terms.Constant(1.0), velocityLengthError)
			);

			Console.WriteLine("objective value");
			Console.WriteLine(objectiveValue);
			Console.WriteLine();

			IEnumerable<Constraint<ValueTerm>> constraintValues =
			(
				Enumerables.Concatenate
				(
					from curveSpecification in curveSpecifications
					from segment in GetAffectedSegments(segments, curveSpecification.Position)
					select Constraints.CreateZero(curveSpecification.GetErrorTerm(segment.GetGlobalCurve())),

					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
					let segment0 = segments.ElementAt(segmentIndex + 0).GetLocalCurve()
					let segment1 = segments.ElementAt(segmentIndex + 1).GetLocalCurve()
					select Constraints.CreateEquality
					(
						segment0.InstantiatePosition(Terms.Constant(1)),
						segment1.InstantiatePosition(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
					let segment0 = segments.ElementAt(segmentIndex + 0).GetLocalCurve().Derivative
					let segment1 = segments.ElementAt(segmentIndex + 1).GetLocalCurve().Derivative
					select Constraints.CreateEquality
					(
						segment0.InstantiatePosition(Terms.Constant(1)),
						segment1.InstantiatePosition(Terms.Constant(0))
					),

					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
					let segment0 = segments.ElementAt(segmentIndex + 0).GetLocalCurve().Derivative.Derivative
					let segment1 = segments.ElementAt(segmentIndex + 1).GetLocalCurve().Derivative.Derivative
					select Constraints.CreateEquality
					(
						segment0.InstantiatePosition(Terms.Constant(1)),
						segment1.InstantiatePosition(Terms.Constant(0))
					)
				)
			)
			.ToArray();
						
			Console.WriteLine("constraint values");
			foreach (Constraint<ValueTerm> constraintValue in constraintValues) Console.WriteLine(constraintValue);
			Console.WriteLine();

			FunctionTerm objectiveFunction = objectiveValue.Abstract(Variables);
			Constraint<FunctionTerm> constraint = Constraints.Merge(constraintValues).Abstract(Variables);

			this.problem = new NlpProblem(objectiveFunction, constraint, new Settings());
		}

		public IEnumerable<Curve> Optimize()
		{
			IEnumerable<Assignment> startAssignments =
			(
				from variable in Variables
				select new Assignment(variable, Enumerable.Repeat(1.0, variable.Dimension))
			)
			.ToArray();

			IEnumerable<Assignment> resultAssignments = Assignment.ValuesToAssignments(Variables, problem.Solve(Assignment.AssignmentsToValues(Variables, startAssignments)));

			Console.WriteLine("start assignments");
			foreach (Assignment assignment in startAssignments) Console.WriteLine(assignment);

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

