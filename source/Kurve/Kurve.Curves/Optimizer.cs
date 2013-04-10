using System;
using System.Linq;
using Kurve.Ipopt;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Krach.Calculus.Terms;
using Krach.Calculus;
using Krach.Calculus.Terms.Composite;
using Krach.Calculus.Abstract;
using Krach.Calculus.Terms.Constraints;
using Krach.Calculus.Terms.Notation;
using Krach.Calculus.Terms.Basic.Definitions;
using Krach.Calculus.Terms.Notation.Custom;
using Krach.Calculus.Rules.Definitions;

namespace Kurve.Curves
{
	public class Optimizer
	{
		readonly Variable velocityLength;
		readonly IEnumerable<Segment> segments;
		readonly Problem problem;

		public IEnumerable<Variable> Variables
		{
			get
			{
				return Enumerables.Concatenate
				(
					Enumerables.Create(velocityLength),
					from segment in segments select segment.Parameter
				);
			}
		}
		
		public Optimizer(IEnumerable<PositionedCurveSpecification> curveSpecifications, CurveTemplate segmentCurveTemplate, int segmentCount)
		{
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");
			if (segmentCurveTemplate == null) throw new ArgumentNullException("segmentCurve");
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");

			this.velocityLength = new Variable(1, "vl");
			this.segments =
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)	
				select new Segment(segmentCurveTemplate, GetPositionTransformation(segmentIndex, segmentCount), segmentIndex)
			)
			.ToArray();

			ValueTerm velocityLengthError = Term.Sum
			(
				from segment in segments
				let derivative = segment.GetLocalCurve().Derivative
				from position in Scalars.GetIntermediateValues(0, 1, 20)
				let segmentVelocity = derivative.InstantiatePosition(Term.Constant(position))
				// TODO: is it okay to square the velocity norm?
				select Term.Square(Term.Difference(Term.Square(Term.Norm(segmentVelocity)), velocityLength))
			);
			ValueTerm specificationError = Term.Sum
			(
				from curveSpecification in curveSpecifications
				from segment in GetAffectedSegments(segments, curveSpecification.Position)
				select curveSpecification.GetErrorTerm(segment.GetGlobalCurve())
			);

			ValueTerm objectiveValue = Term.Sum
			(
				Term.Product(Term.Constant(0.1), velocityLengthError),
				Term.Product(Term.Constant(1.0), specificationError)
			);

			objectiveValue = Rewriting.CompleteSimplification.Rewrite(objectiveValue);

			Console.WriteLine("objective value");
			Console.WriteLine(objectiveValue);
			Console.WriteLine();

			IEnumerable<IConstraint<ValueTerm>> constraintValues =
			(
				Enumerables.Concatenate
				(
					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
					select CreatePointConnection(segments.ElementAt(segmentIndex + 0), segments.ElementAt(segmentIndex + 1)),
					from segmentIndex in Enumerable.Range(0, segmentCount - 1)
					select CreateVelocityConnection(segments.ElementAt(segmentIndex + 0), segments.ElementAt(segmentIndex + 1))
				)
			)
			.ToArray();

			constraintValues = constraintValues.Select(constraintValue => Rewriting.CompleteSimplification.Rewrite(constraintValue)).ToArray();
			
			Console.WriteLine("constraint values");
			foreach (IConstraint<ValueTerm> constraintValue in constraintValues) Console.WriteLine(constraintValue);
			Console.WriteLine();

			FunctionTerm objectiveFunction = objectiveValue.Abstract(Variables);
			IConstraint<FunctionTerm> constraint = (constraintValues.Any() ? Constraint.Merge(constraintValues) : Constraint.CreateEmpty()).Abstract(Variables);

			this.problem = new Problem(objectiveFunction.Normalize(2), constraint.Normalize(2), new Settings());
		}

		public IEnumerable<Curve> Optimize()
		{
			IEnumerable<Assignment> startAssignments =
			(
				from variable in Variables
				select new Assignment(variable, Enumerable.Repeat(0.0, variable.Dimension))
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
				select segment.Instantiate(Term.Constant(value))
			)
			.ToArray();

			Console.WriteLine("result curves");
			foreach (Curve curve in resultCurves) Console.WriteLine(curve);
			
			return resultCurves;
		}
		
		static IConstraint<ValueTerm> CreatePointConnection(Segment end, Segment start)
		{
			return Constraint.CreateEquality
			(
				end.GetLocalCurve().InstantiatePosition(Term.Constant(1)),
				start.GetLocalCurve().InstantiatePosition(Term.Constant(0))
			);
		}
		static IConstraint<ValueTerm> CreateVelocityConnection(Segment end, Segment start)
		{
			return Constraint.CreateEquality
			(
				end.GetLocalCurve().Derivative.InstantiatePosition(Term.Constant(1)),
				start.GetLocalCurve().Derivative.InstantiatePosition(Term.Constant(0))
			);
		}

		static FunctionTerm GetPositionTransformation(int segmentIndex, int segmentCount)
		{
			Variable position = new Variable(1, "t");

			return new FunctionDefinition
			(
				string.Format("position_transformation_{0}_{1}", segmentIndex, segmentCount),
				Term.Difference(Term.Product(Term.Constant(segmentCount), position), Term.Constant(segmentIndex)).Abstract(position),
				new BasicSyntax(string.Format("τ_{0}", segmentIndex))
			);
		}
		static IEnumerable<Segment> GetAffectedSegments(IEnumerable<Segment> segments, double position)
		{
			return
			(
				from segment in segments
				let localPosition = segment.PositionTransformation.Evaluate(Enumerables.Create(position)).Single()
				where new OrderedRange<double>(0, 1).Contains(localPosition)
				select segment
			)
			.ToArray();
		}
	}
}

