using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves
{
	public class OptimizationProblem
	{
		readonly BasicSpecification basicSpecification;
		readonly IEnumerable<Segment> segments;
		readonly IEnumerable<ValueTerm> variables;
		readonly ValueTerm curveLength;
		readonly int pointSpecificationCount;
		readonly IEnumerable<ValueTerm> pointSpecificationPositions;
		readonly IEnumerable<ValueTerm> pointSpecificationPoints;
		readonly IEnumerable<IEnumerable<ValueTerm>> pointSpecificationSegmentWeights;
		readonly IpoptProblem problem;
		readonly IEnumerable<OrderedRange<double>> constraints;

		OptimizationProblem(BasicSpecification basicSpecification)
		{
			this.basicSpecification = basicSpecification;

			this.segments =
			(
				from segmentIndex in Enumerable.Range(0, basicSpecification.SegmentCount)	
				select new Segment(basicSpecification.SegmentTemplate, GetPositionTransformation(segmentIndex, basicSpecification.SegmentCount), segmentIndex)
			)
			.ToArray();

			this.variables =
			(
				from segment in segments
				select segment.Parameter
			)
			.ToArray();

			this.curveLength = Terms.Variable("curveLength");

			ValueTerm speedError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.GetLocalCurve()
				let segmentLength = Terms.Quotient(curveLength, Terms.Constant(basicSpecification.SegmentCount))
				let segmentError = Terms.Square(Terms.Difference(Terms.Norm(segmentCurve.Velocity.Apply(position)), segmentLength))
				select Terms.IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 1)
			);
			ValueTerm fairnessError = Terms.Sum
			(
				from segment in segments
				let position = Terms.Variable("t")
				let segmentCurve = segment.GetLocalCurve()
				let segmentError = Terms.Square(Terms.Norm(segmentCurve.Acceleration.Apply(position)))
				select Terms.IntegrateTrapezoid(segmentError.Abstract(position), new OrderedRange<double>(0, 1), 1)
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

			this.pointSpecificationCount = basicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification);
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
							select Terms.Scaling(segmentWeight, PointCurveSpecification.GetErrorTerm(segment.GetGlobalCurve(), position, point))
						)
					),

					//Enumerables.Create(Constraints.CreateEquality(Terms.Constant(0), Terms.Constant(0)))

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

			this.problem = IpoptProblem.Create(objectiveFunction, constraintFunction);
			this.constraints = constraint.Ranges;
		}

		public OptimizationProblem AdjustSize(BasicSpecification newBasicSpecification)
		{
			if (newBasicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification) > pointSpecificationCount)
				return new OptimizationProblem(newBasicSpecification);

			return this;
		}
		public IpoptSolver GetProblem(BasicSpecification newBasicSpecification)
		{
			if (newBasicSpecification.SegmentCount != basicSpecification.SegmentCount) throw new ArgumentException();
			if (newBasicSpecification.SegmentTemplate != basicSpecification.SegmentTemplate) throw new ArgumentException();
			if (newBasicSpecification.CurveSpecifications.Count(curveSpecification => curveSpecification is PointCurveSpecification) > pointSpecificationCount) throw new ArgumentException();

			IEnumerable<Substitution> substitutions = GetSubstitutions(newBasicSpecification).ToArray();

			IpoptProblem instantiatedProblem = problem.Substitute(substitutions);

			return new IpoptSolver(instantiatedProblem, constraints, new Settings());
		}
		public IEnumerable<Curve> GetCurves(IEnumerable<double> position)
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

		IEnumerable<Substitution> GetSubstitutions(BasicSpecification newBasicSpecification)
		{
			yield return new Substitution(curveLength, Terms.Constant(newBasicSpecification.CurveLength));

			foreach (int pointSpecificationIndex in Enumerable.Range(0, pointSpecificationCount))
			{
				ValueTerm position = pointSpecificationPositions.ElementAt(pointSpecificationIndex);
				ValueTerm point = pointSpecificationPoints.ElementAt(pointSpecificationIndex);
				IEnumerable<ValueTerm> segmentWeights = pointSpecificationSegmentWeights.ElementAt(pointSpecificationIndex);

				if (pointSpecificationIndex < newBasicSpecification.CurveSpecifications.Count())
				{
					PointCurveSpecification pointCurveSpecification = (PointCurveSpecification)newBasicSpecification.CurveSpecifications.ElementAt(pointSpecificationIndex);
					
					yield return new Substitution(position, Terms.Constant(pointCurveSpecification.Position));
					yield return new Substitution(point, Terms.Constant(pointCurveSpecification.Point.X, pointCurveSpecification.Point.Y));
					foreach (int segmentIndex in Enumerable.Range(0, segments.Count()))
					{
						Segment segment = segments.ElementAt(segmentIndex);
						double localPosition = segment.PositionTransformation.Apply(Terms.Constant(pointCurveSpecification.Position)).Evaluate().Single();
						double segmentWeight = new OrderedRange<double>(0, 1).Contains(localPosition) ? 1 : 0;

						yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(segmentWeight));
					}
				}
				else
				{
					yield return new Substitution(position, Terms.Constant(0));
					yield return new Substitution(point, Terms.Constant(0, 0));
					foreach (int segmentIndex in Enumerable.Range(0, segments.Count()))
						yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(0));
				}
			}
		}

		public static OptimizationProblem Create(BasicSpecification newBasicSpecification)
		{
			return new OptimizationProblem(newBasicSpecification);
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

