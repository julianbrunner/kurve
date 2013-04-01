using System;
using System.Linq;
using Kurve.Ipopt;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Kurve.Curves.Segmentation;
using Kurve.Curves.Specification;
using Krach.Calculus.Terms;
using Krach.Calculus;
using Krach.Calculus.Terms.Composite;

namespace Kurve.Curves
{
	public class Optimizer
	{
		readonly Curve segmentCurve;
		readonly IEnumerable<Segment> segments;
		readonly IEnumerable<CurveConstraint> constraints;
		readonly IEnumerable<Variable> variables;
		readonly Problem problem;
		
		public Optimizer(IEnumerable<CurveSpecification> curveSpecifications, Curve segmentCurve, int segmentCount)
		{
			if (curveSpecifications == null) throw new ArgumentNullException("curveSpecifications");
			if (segmentCurve == null) throw new ArgumentNullException("segmentCurve");
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");

			this.segmentCurve = segmentCurve;
			this.segments =
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				let segmentParameter = new Variable(segmentCurve.ParameterDimension, string.Format("sp_{0}", segmentIndex))
				let segmentPositionTransformation = GetPositionTransformation(segmentIndex, segmentCount)
				select new Segment(segmentCurve, segmentParameter, segmentPositionTransformation)
			)
			.ToArray();
			this.constraints =
			(
				from segmentIndex in Enumerable.Range(0, segmentCount - 1)
				select CurveConstraint.FromPointConnection
				(
					segments.ElementAt(segmentIndex + 0),
					segments.ElementAt(segmentIndex + 1)
				)
			)
			.ToArray();
			this.variables =
			(
				from segment in segments
				select segment.Parameter
			)
			.ToArray();

			IFunction objectiveFunction = Term.Sum
			(
				from curveSpecification in curveSpecifications
				let segmentIndex = (int)(curveSpecification.Position * segmentCount)
				let segment = segmentIndex == segmentCount ? segments.Last() : segments.ElementAt(segmentIndex)
				select curveSpecification.GetErrorTerm(segment.GetGlobalCurve())
			)
			.Abstract(variables)
			.Normalize(2);

			IFunction constraintFunction = Term.Vector
			(
				from curveConstraint in constraints
				select curveConstraint.Value
			)
			.Abstract(variables)
			.Normalize(2);

			Constraint constraint = new Constraint
			(
				constraintFunction,
				(
					from curveConstraint in constraints
					from range in curveConstraint.Ranges
					select range
				)
				.ToArray()
			);

			this.problem = new Problem(objectiveFunction, constraint, new Settings());
		}

		public IEnumerable<Curve> Optimize()
		{
			IEnumerable<Assignment> startAssignments =
			(
				from variable in variables
				select new Assignment(variable, Enumerable.Repeat(0.0, variable.Dimension))
			)
			.ToArray();

			IEnumerable<Assignment> resultAssignments = ValuesToAssignments(variables, problem.Solve(AssignmentsToValues(variables, startAssignments)));

			Console.WriteLine("start assignments");
			foreach (Assignment assignment in startAssignments) Console.WriteLine(assignment);

			Console.WriteLine("result assignments");
			foreach (Assignment assignment in resultAssignments) Console.WriteLine(assignment);

			IEnumerable<Curve> resultCurves =
			(
				from segment in segments
				let value = resultAssignments.Single(assignment => assignment.Variable == segment.Parameter).Value
				select new Curve(Rewriting.CompleteSimplification.Rewrite(segmentCurve.Instantiate(Term.Constant(value)).Function))
			)
			.ToArray();

			Console.WriteLine("result curves");
			foreach (Curve curve in resultCurves) Console.WriteLine(curve);
			
			return resultCurves;
		}

		static FunctionTerm GetPositionTransformation(int segmentIndex, int segmentCount)
		{
			Variable position = new Variable(1, "t");

			return Term.Difference(Term.Product(Term.Constant(segmentCount), position), Term.Constant(segmentIndex)).Abstract(position);
		}
		static IEnumerable<double> AssignmentsToValues(IEnumerable<Variable> variables, IEnumerable<Assignment> assignments)
		{
			return
			(
				from assignment in assignments
				from value in assignment.Value
				select value
			)
			.ToArray();
		}
		static IEnumerable<Assignment> ValuesToAssignments(IEnumerable<Variable> variables, IEnumerable<double> values)
		{
			return Enumerables.Zip
			(
				variables,
				variables.Select(variable => variable.Dimension).GetPartialSums(),
				variables.Select(variable => variable.Dimension),
				(variable, start, length) => new Assignment(variable, values.Skip(start).Take(length).ToArray())
			)
			.ToArray();
		}
	}
}

