using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach;

namespace Kurve.Curves
{
	class SpecificationTemplate
	{
		readonly ValueTerm position;
		readonly ValueTerm value;
		readonly IEnumerable<ValueTerm> segmentWeights;
		readonly Constraint<ValueTerm> constraint;

		public Constraint<ValueTerm> Constraint { get { return constraint; } }

		SpecificationTemplate(ValueTerm position, ValueTerm value, IEnumerable<ValueTerm> segmentWeights, Constraint<ValueTerm> constraint)
		{
			if (position == null) throw new ArgumentNullException("position");
			if (value == null) throw new ArgumentNullException("value");
			if (segmentWeights == null) throw new ArgumentNullException("segmentWeights");
			if (constraint == null) throw new ArgumentNullException("constraint");

			this.position = position;
			this.value = value;
			this.segmentWeights = segmentWeights;
			this.constraint = constraint;
		}

		public IEnumerable<Substitution> GetSubstitutions(IEnumerable<Segment> segments)
		{
			yield return new Substitution(position, Terms.Constant(0));
			yield return new Substitution(value, Terms.Constant(Enumerable.Repeat(0.0, value.Dimension)));

			foreach (int segmentIndex in Enumerable.Range(0, segments.Count())) yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(0));
		}
		public IEnumerable<Substitution> GetSubstitutions(IEnumerable<Segment> segments, double specificationPosition, ValueTerm substitutionPosition, ValueTerm substitutionValue)
		{			
			yield return new Substitution(position, substitutionPosition);
			yield return new Substitution(value, substitutionValue);

			foreach (int segmentIndex in Enumerable.Range(0, segments.Count()))
			{
				Segment segment = segments.ElementAt(segmentIndex);
				double segmentWeight = segment.Contains(specificationPosition) ? 1 : 0;

				yield return new Substitution(segmentWeights.ElementAt(segmentIndex), Terms.Constant(segmentWeight));
			}
		}

		// TODO: remove duplication regarding the different specification types
		public static SpecificationTemplate CreatePointSpecificationTemplate(IEnumerable<Segment> segments, int index)
		{
			ValueTerm position = Terms.Variable(string.Format("p_{0}_position", index));
			ValueTerm point = Terms.Variable(string.Format("p_{0}_point", index), 2);
			IEnumerable<ValueTerm> segmentWeights =
			(
				from segmentIndex in Enumerable.Range(0, segments.Count())
				select Terms.Variable(string.Format("p_{0}_segment_weight_{1}", index, segmentIndex))
			)
			.ToArray();
			Constraint<ValueTerm> constraint = Constraints.CreateZero
			(
				Terms.Sum
				(
					Enumerable.Zip
					(
						segments,
						segmentWeights,
						(segment, segmentWeight) => Terms.Scaling(segmentWeight, Terms.Difference(segment.GlobalCurve.Point.Apply(position), point))
					)
				)
			);

			return new SpecificationTemplate(position, point, segmentWeights, constraint);
		}
		public static SpecificationTemplate CreateDirectionSpecificationTemplate(IEnumerable<Segment> segments, int index)
		{
			ValueTerm position = Terms.Variable(string.Format("d_{0}_position", index));
			ValueTerm direction = Terms.Variable(string.Format("d_{0}_direction", index), 2);
			IEnumerable<ValueTerm> segmentWeights =
			(
				from segmentIndex in Enumerable.Range(0, segments.Count())
				select Terms.Variable(string.Format("d_{0}_segment_weight_{1}", index, segmentIndex))
			)
			.ToArray();
			Constraint<ValueTerm> constraint = Constraints.CreateZero
			(
				Terms.Sum
				(
					Enumerable.Zip
					(
						segments,
						segmentWeights,
						(segment, segmentWeight) => Terms.Scaling(segmentWeight, Terms.Difference(segment.GlobalCurve.NormalizedVelocity.Apply(position), direction))
					)
				)
			);

			return new SpecificationTemplate(position, direction, segmentWeights, constraint);
		}
		public static SpecificationTemplate CreateCurvatureSpecificationTemplate(IEnumerable<Segment> segments, int index)
		{
			ValueTerm position = Terms.Variable(string.Format("c_{0}_position", index));
			ValueTerm curvature = Terms.Variable(string.Format("c_{0}_curvature", index), 1);
			IEnumerable<ValueTerm> segmentWeights =
			(
				from segmentIndex in Enumerable.Range(0, segments.Count())
				select Terms.Variable(string.Format("c_{0}_segment_weight_{1}", index, segmentIndex))
			)
			.ToArray();
			Constraint<ValueTerm> constraint = Constraints.CreateZero
			(
				Terms.Sum
				(
					Enumerable.Zip
					(
						segments,
						segmentWeights,
						(segment, segmentWeight) => Terms.Scaling(segmentWeight, Terms.Difference(segment.GlobalCurve.Curvature.Apply(position), curvature))
					)
				)
			);

			return new SpecificationTemplate(position, curvature, segmentWeights, constraint);
		}
	}
}

