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

			this.parameters = 
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				select Terms.Variable(string.Format("sp_{0}", segmentIndex), segmentTemplate.ParameterDimension)
			)
			.ToArray();
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
			IEnumerable<ValueTerm> parameters =
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				select Terms.Constant(values.Skip(segmentIndex * segmentTemplate.ParameterDimension).Take(segmentTemplate.ParameterDimension))
			)
			.ToArray();

			return new SegmentedCurve(GetSegments(segmentCount, segmentTemplate, parameters));
		}

		public static OptimizationSegments Create(Specification specification)
		{
			return new OptimizationSegments
			(
				specification.BasicSpecification.SegmentCount,
				specification.BasicSpecification.SegmentTemplate
			);
		}

		static IEnumerable<Segment> GetSegments(int segmentCount, FunctionTermCurveTemplate segmentTemplate, IEnumerable<ValueTerm> parameters)
		{
			return
			(
				from segmentIndex in Enumerable.Range(0, segmentCount)
				let parameter = parameters.ElementAt(segmentIndex)
				let curve = segmentTemplate.InstantiateParameter(parameter)
				let positionTransformation = GetPositionTransformation(segmentCount, segmentIndex)
				select new Segment(curve, positionTransformation)
			)
			.ToArray();
		}
		static FunctionTerm GetPositionTransformation(int segmentCount, int segmentIndex)
		{
			ValueTerm position = Terms.Variable("t");
			
			return Terms.Difference(Terms.Product(Terms.Constant(segmentCount), position), Terms.Constant(segmentIndex)).Abstract(position);
		}
	}
}

