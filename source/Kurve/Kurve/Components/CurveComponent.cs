using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Krach.Graphics;
using Kurve.Curves;
using Kurve.Curves.Optimization;
using Krach.Maps.Abstract;
using System.Diagnostics;
using Krach.Maps.Scalar;
using Krach.Maps;
using Kurve.Interface;
using Cairo;

namespace Kurve.Component
{
	class CurveComponent : Component
	{
		readonly CurveOptimizer curveOptimizer;

		readonly List<PointSpecificationComponent> pointSpecificationComponents;
		readonly List<SegmentComponent> interSpecificationComponents;

		BasicSpecification nextSpecification;

		BasicSpecification basicSpecification;
		Curve curve;

		IEnumerable<PositionedControlComponent> PositionedControlComponents
		{
			get
			{
				return Enumerables.Concatenate<PositionedControlComponent>
				(
					pointSpecificationComponents,
					interSpecificationComponents
				);
			}
		}
		IEnumerable<SpecificationComponent> SpecificationComponents
		{
			get
			{
				return Enumerables.Concatenate<SpecificationComponent>
				(
					pointSpecificationComponents
				);
			}
		}
		IEnumerable<SegmentComponent> InterSpecificationComponents
		{
			get
			{
				return Enumerables.Concatenate<SegmentComponent>
				(
					interSpecificationComponents
				);
			}
		}

		protected override IEnumerable<Component> SubComponents
		{
			get
			{
				return Enumerables.Concatenate<Component>
				(
					interSpecificationComponents,
					pointSpecificationComponents
				);
			}
		}

		public BasicSpecification BasicSpecification { get { return basicSpecification; } }
		public Curve Curve { get { return curve; } }
		public Specification Specification { get { return curveOptimizer.Specification; } }

		public CurveComponent(Component parent, OptimizationWorker optimizationWorker, Specification specification) : base(parent)
		{
			if (optimizationWorker == null) throw new ArgumentNullException("optimizationWorker");

			this.curveOptimizer = new CurveOptimizer(optimizationWorker, specification);
			this.curveOptimizer.CurveChanged += CurveChanged;
			
			this.pointSpecificationComponents = new List<PointSpecificationComponent>();
			this.interSpecificationComponents = new List<SegmentComponent>();

			nextSpecification = specification.BasicSpecification;
			curve = null;

			RebuildInterSpecificationComponents();

			curveOptimizer.Submit(nextSpecification);

			foreach (PointCurveSpecification pointCurveSpecification in nextSpecification.CurveSpecifications.OfType<PointCurveSpecification>())
				AddPointSpecificationComponent(new PointSpecificationComponent(this, this, pointCurveSpecification.Position, pointCurveSpecification.Point));
		}

		void CurveChanged(BasicSpecification newBasicSpecification, Curve newCurve)
		{
			basicSpecification = newBasicSpecification;
			curve = newCurve;
			
			Changed();
		}

		void InsertLength(double length)
		{
			if (PositionedControlComponents.Any(positionedControlComponent => positionedControlComponent.Selected)) return;

			double newCurveLength = Comparables.Maximum(1, nextSpecification.CurveLength + length);

			ChangeCurveLength(newCurveLength);
			
			curveOptimizer.Submit(nextSpecification);
		}
		void InsertLength(double position, double length)
		{
			double newCurveLength = Comparables.Maximum(1, nextSpecification.CurveLength + length);
			double lengthRatio = nextSpecification.CurveLength / newCurveLength;

			ChangeCurveLength(newCurveLength);

			foreach (SpecificationComponent specificationComponent in SpecificationComponents)
				specificationComponent.CurrentPosition = ShiftPosition(specificationComponent.CurrentPosition, position, lengthRatio);
			
			RebuildCurveSpecification();

			curveOptimizer.Submit(nextSpecification);
		}
		void SpecificationChanged()
		{
			RebuildCurveSpecification();

			curveOptimizer.Submit(nextSpecification);
		}

		void ChangeCurveLength(double newCurveLength)
		{
			nextSpecification = new BasicSpecification
			(
				newCurveLength,
				nextSpecification.SegmentCount,
				nextSpecification.SegmentTemplate,
				nextSpecification.CurveSpecifications
			);
		}
		void RebuildCurveSpecification()
		{
			nextSpecification = new BasicSpecification
			(
				nextSpecification.CurveLength,
				nextSpecification.SegmentCount,
				nextSpecification.SegmentTemplate,
				(
					from pointSpecificationComponent in pointSpecificationComponents
					select new PointCurveSpecification(pointSpecificationComponent.Position, pointSpecificationComponent.Point)
				)
				.ToArray()
			);
		}

		void AddPointSpecificationComponent(PointSpecificationComponent pointSpecificationComponent)
		{
			pointSpecificationComponent.SpecificationChanged += SpecificationChanged;
			pointSpecificationComponent.InsertLength += InsertLength;

			pointSpecificationComponents.Add(pointSpecificationComponent);

			RebuildInterSpecificationComponents();

			Changed();

			RebuildCurveSpecification();

			curveOptimizer.Submit(nextSpecification);
		}
		void RemovePointSpecificationComponent(PointSpecificationComponent pointSpecificationComponent)
		{
			pointSpecificationComponents.Remove(pointSpecificationComponent);

			RebuildInterSpecificationComponents();

			Changed();

			RebuildCurveSpecification();

			curveOptimizer.Submit(nextSpecification);
		}

		void AddSpecification(double position)
		{
			PointSpecificationComponent pointSpecificationComponent = new PointSpecificationComponent(this, this, position, Curve.GetPoint(position));

			AddPointSpecificationComponent(pointSpecificationComponent);
		}
		void AddPointSpecificationComponent()
		{
			IEnumerable<SegmentComponent> selectedInterSpecificationComponents =
			(
				from interSpecificationComponent in InterSpecificationComponents
				where interSpecificationComponent.Selected
				select interSpecificationComponent
			)
			.ToArray();

			foreach (SegmentComponent interSpecificationComponent in selectedInterSpecificationComponents)
			{
				PointSpecificationComponent pointSpecificationComponent = new PointSpecificationComponent(this, this, interSpecificationComponent.Position, interSpecificationComponent.Point);

				AddPointSpecificationComponent(pointSpecificationComponent);
			}
		}
		void RemovePointSpecificationComponent()
		{
			IEnumerable<SpecificationComponent> selectedSpecificationComponents =
			(
				from specificationComponent in SpecificationComponents
				where specificationComponent.Selected
				select specificationComponent
			)
			.ToArray();

			foreach (SpecificationComponent specificationComponent in selectedSpecificationComponents)
			{
				RemovePointSpecificationComponent((PointSpecificationComponent)specificationComponent);
			}
		}

		void RebuildInterSpecificationComponents()
		{
			IEnumerable<SpecificationComponent> orderedSpecificationComponents =
			(
				from specificationComponent in SpecificationComponents
				orderby specificationComponent.Position ascending
				select specificationComponent
			)
			.ToArray();

			interSpecificationComponents.Clear();

			foreach (Tuple<SpecificationComponent, SpecificationComponent> specificationComponentRange in orderedSpecificationComponents.GetRanges())
			{
				SegmentComponent interSpecificationComponent = new SegmentComponent(this, this, specificationComponentRange.Item1, specificationComponentRange.Item2);

				interSpecificationComponent.InsertLength += InsertLength;
				interSpecificationComponent.SpecificationChanged += SpecificationChanged;
				interSpecificationComponent.AddSpecification += AddSpecification;
				interSpecificationComponents.Add(interSpecificationComponent);
			}
		}

		public override void KeyDown(Key key)
		{
			base.KeyDown(key);
		}
		public override void KeyUp(Key key)
		{
			switch (key)
			{
				case Key.A: AddPointSpecificationComponent(); break;
				case Key.R: RemovePointSpecificationComponent(); break;
			}

			base.KeyUp(key);
		}

		static double ShiftPosition(double position, double insertionPosition, double lengthRatio)
		{
			if (position == insertionPosition) return position;
			if (position < insertionPosition) return position * lengthRatio;
			if (position > insertionPosition) return 1 - (1 - position) * lengthRatio;

			throw new InvalidOperationException();
		}
	}
}

