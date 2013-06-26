using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;

namespace Kurve.Curves
{
	class OptimizationSegments
	{
		readonly int segmentCount;
		readonly FunctionTermCurveTemplate segmentTemplate;

		readonly IEnumerable<ValueTerm> parameters;
		readonly IEnumerable<Segment> segments;

		public IEnumerable<ValueTerm> Parameters { get { return parameters; } }
		public IEnumerable<Segment> Segments { get { return segments; } }

		OptimizationSegments(int segmentCount, FunctionTermCurveTemplate segmentTemplate)
		{
			if (segmentCount < 0) throw new ArgumentOutOfRangeException("segmentCount");
			if (segmentTemplate == null) throw new ArgumentNullException("segmentTemplate");

			this.segmentCount = segmentCount;
			this.segmentTemplate = segmentTemplate;

			this.parameters = GetParameters(segmentCount, segmentTemplate);
			this.segments = GetSegments(segmentCount, segmentTemplate, parameters);
		}

		public bool NeedsRebuild(Specification newSpecification)
		{
			return
				segmentCount != newSpecification.BasicSpecification.SegmentCount ||
				segmentTemplate != newSpecification.BasicSpecification.SegmentTemplate;
		}

		public Curve GetCurve(IEnumerable<double> values)
		{
			IEnumerable<ValueTerm> parameters = GetValues(segmentCount, segmentTemplate, values);
			IEnumerable<Segment> segments = GetSegments(segmentCount, segmentTemplate, parameters);

			Func<double, int> getSegmentIndex = delegate (double position)
			{
				if (position == 1) return segmentCount - 1;

				return (int)(position * segmentCount);
			};

			return new SegmentedCurve(segments, getSegmentIndex);
		}

		public static OptimizationSegments Create(Specification specification)
		{
			return new OptimizationSegments
			(
				specification.BasicSpecification.SegmentCount,
				specification.BasicSpecification.SegmentTemplate
			);
		}

		static IEnumerable<ValueTerm> GetParameters(int segmentCount, FunctionTermCurveTemplate segmentTemplate)
		{
			return
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				select Terms.Variable(string.Format("sp_{0}", segmentIndex), segmentTemplate.ParameterDimension)
			)
			.ToArray();
		}
		static IEnumerable<ValueTerm> GetValues(int segmentCount, FunctionTermCurveTemplate segmentTemplate, IEnumerable<double> values)
		{
			return
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				select Terms.Constant(values.Skip(segmentIndex * segmentTemplate.ParameterDimension).Take(segmentTemplate.ParameterDimension))
			)
			.ToArray();
		}
		static IEnumerable<Segment> GetSegments(int segmentCount, FunctionTermCurveTemplate segmentTemplate, IEnumerable<ValueTerm> parameters)
		{
			return
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				let parameter = parameters.ElementAt(segmentIndex)
				let curve = segmentTemplate.InstantiateParameter(parameter)
				let position = Terms.Variable("t")
				let positionTransformation = Terms.Difference(Terms.Product(Terms.Constant(segmentCount), position), Terms.Constant(segmentIndex)).Abstract(position)
				select new Segment(curve, positionTransformation)
			)
			.ToArray();
		}
	}
}

