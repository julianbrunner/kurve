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
		
		public CurveOptimizer CurveOptimizer { get { return curveOptimizer; } }
		
		readonly List<SpecificationComponent> specificationComponents;
		readonly List<SegmentComponent> segmentComponents;
		readonly FixedPositionComponent curveStartComponent;
		readonly FixedPositionComponent curveEndComponent;

		BasicSpecification nextSpecification;

		BasicSpecification basicSpecification;
		Curve curve;

		bool isShiftDown = false;
		bool isAltDown = false;

		IEnumerable<PositionedControlComponent> PositionedControlComponents
		{
			get
			{
				return Enumerables.Concatenate<PositionedControlComponent>
				(
					specificationComponents,
					segmentComponents
				);
			}
		}
		IEnumerable<SpecificationComponent> SpecificationComponents
		{
			get
			{
				return Enumerables.Concatenate<SpecificationComponent>
				(
					specificationComponents
				);
			}
		}
		IEnumerable<PositionedControlComponent> SegmentDelimitingComponents 
		{
			get 
			{
				return Enumerables.Concatenate<PositionedControlComponent>
				(
					SpecificationComponents,
					Enumerables.Create(curveStartComponent, curveEndComponent)
				);
			}
		}
		IEnumerable<SegmentComponent> SegmentComponents
		{
			get
			{
				return Enumerables.Concatenate<SegmentComponent>
				(
					segmentComponents
				);
			}
		}

		bool Selected
		{
			get
			{
				return
					specificationComponents.Any(specificationComponent => specificationComponent.IsSelected) ||
					segmentComponents.Any(segmentComponent => segmentComponent.IsSelected);
			}
		}

		protected override IEnumerable<Component> SubComponents
		{
			get
			{
				return Enumerables.Concatenate<Component>
				(
					segmentComponents,
					specificationComponents
				);
			}
		}

		public event Action RemoveCurve;

		public BasicSpecification BasicSpecification { get { return basicSpecification; } }
		public Curve Curve { get { return curve; } }
		public Specification Specification { get { return curveOptimizer.Specification; } }

		public CurveComponent(Component parent, OptimizationWorker optimizationWorker, Specification specification) : base(parent)
		{
			if (optimizationWorker == null) throw new ArgumentNullException("optimizationWorker");

			this.curveOptimizer = new CurveOptimizer(optimizationWorker, specification);
			this.curveOptimizer.CurveChanged += CurveChanged;

			this.curveStartComponent = new FixedPositionComponent(this, this, 0);
			this.curveEndComponent = new FixedPositionComponent(this, this, 1);
			this.specificationComponents = new List<SpecificationComponent>();
			this.segmentComponents = new List<SegmentComponent>();

			nextSpecification = specification.BasicSpecification;
			curve = null;

			RebuildSegmentComponents();

			curveOptimizer.Submit(nextSpecification);

			IEnumerable<SpecificationComponent> specificationComponents = 
				from spec in nextSpecification.CurveSpecifications
				orderby spec.Position ascending
				group spec by spec.Position into specificationGroup
				select new SpecificationComponent(this, this, specificationGroup.Key, specificationGroup);

			foreach (SpecificationComponent component in specificationComponents) AddSpecificationComponent(component);
		}

		void CurveChanged(BasicSpecification newBasicSpecification, Curve newCurve)
		{
			basicSpecification = newBasicSpecification;
			curve = newCurve;
			
			Changed();
		}

		void InsertLength(double length)
		{
			if (PositionedControlComponents.Any(positionedControlComponent => positionedControlComponent.IsSelected)) return;

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
					from specificationComponent in SpecificationComponents
					from specification in specificationComponent.Specifications
					select specification
				)
				.ToArray()
			);
		}

		void AddSpecificationComponent(SpecificationComponent specificationComponent)
		{
			specificationComponent.SpecificationChanged += SpecificationChanged;
			specificationComponent.InsertLength += InsertLength;
			specificationComponent.SelectionChanged += SelectionChanged;
			specificationComponents.Add(specificationComponent);

			RebuildSegmentComponents();

			Changed();

			RebuildCurveSpecification();

			curveOptimizer.Submit(nextSpecification);
		}
		void RemoveSelectedSpecificationComponent(SpecificationComponent pointSpecificationComponent)
		{
			specificationComponents.Remove(pointSpecificationComponent);

			if (!specificationComponents.Any() && RemoveCurve != null) RemoveCurve();
			else
			{
				RebuildSegmentComponents();

				Changed();

				RebuildCurveSpecification();

				curveOptimizer.Submit(nextSpecification);
			}
		}

		void AddSpecification(double position)
		{
			SpecificationComponent pointSpecificationComponent = new SpecificationComponent(this, this, position, Enumerables.Create<CurveSpecification>());

			AddSpecificationComponent(pointSpecificationComponent);
		}

		void RemoveSelectedSpecificationComponent()
		{
			IEnumerable<SpecificationComponent> selectedSpecificationComponents =
			(
				from specificationComponent in SpecificationComponents
				where specificationComponent.IsSelected
				select specificationComponent
			)
			.ToArray();

			foreach (SpecificationComponent specificationComponent in selectedSpecificationComponents)
			{
				RemoveSelectedSpecificationComponent(specificationComponent);
			}
		}

		void ChangeCurveSegmentCount(int newSegmentCount)
		{
			if (Selected)
			{
				if (newSegmentCount < 1)
				{
					Console.WriteLine("segment count cannot be smaller than one!");

					return;
				}

				Console.WriteLine("changing segment count to {0}", newSegmentCount);

				nextSpecification = new BasicSpecification
				(
					nextSpecification.CurveLength,
					newSegmentCount,
					nextSpecification.SegmentTemplate,
					nextSpecification.CurveSpecifications
				);
				
				RebuildCurveSpecification();

				curveOptimizer.Submit(nextSpecification);
			}
		}
		void ChangePolynomialtemplateDegree(int newDegree)
		{
			if (Selected)
			{
				if (newDegree < 4)
				{
					Console.WriteLine("polynomial template degree cannot be less than 4");

					return;
				}

				Console.WriteLine("changing polynomial template degree to {0}", newDegree);

				nextSpecification = new BasicSpecification
				(
					nextSpecification.CurveLength,
					nextSpecification.SegmentCount,
					new PolynomialFunctionTermCurveTemplate(newDegree),
					nextSpecification.CurveSpecifications
				);
				
				RebuildCurveSpecification();

				curveOptimizer.Submit(nextSpecification);
			}
		}


		void RebuildSegmentComponents()
		{
			IEnumerable<PositionedControlComponent> orderedSpecificationComponents =
			(
				from specificationComponent in SpecificationComponents
				orderby specificationComponent.Position ascending
				select specificationComponent
			)
			.ToArray();

			segmentComponents.Clear();

			IEnumerable<PositionedControlComponent> segmentDelimitingComponents = Enumerables.Concatenate
			(
				Enumerables.Create(curveStartComponent),
				orderedSpecificationComponents,
				Enumerables.Create(curveEndComponent)
			);

			foreach (Tuple<PositionedControlComponent, PositionedControlComponent> segmentDelimitingComponentRange in segmentDelimitingComponents.GetRanges())
			{
				SegmentComponent segmentComponent = new SegmentComponent(this, this, segmentDelimitingComponentRange.Item1, segmentDelimitingComponentRange.Item2);

				segmentComponent.InsertLength += InsertLength;
				segmentComponent.SpecificationChanged += SpecificationChanged;
				segmentComponent.AddSpecification += AddSpecification;
				segmentComponent.SelectionChanged += SelectionChanged;

				segmentComponents.Add(segmentComponent);
			}
		}

		public void SelectionChanged(PositionedControlComponent selectedComponent)
		{
			if (selectedComponent.IsSelected && !isShiftDown)
			{
				foreach (PositionedControlComponent component in PositionedControlComponents.Except(Enumerables.Create(selectedComponent))) 
					component.IsSelected = false;

				Changed();
			}
		}

		public override void KeyDown(Key key)
		{
			if (key == Key.Shift) isShiftDown = true;
			if (key == Key.Alt) isAltDown = true;

			base.KeyDown(key);
		}
		public override void KeyUp(Key key)
		{
			switch (key)
			{
				case Key.Shift: isShiftDown = false; break;
				case Key.Alt: isAltDown = false; break;
				case Key.R: RemoveSelectedSpecificationComponent(); break;
				case Key.One:
					if (isAltDown) ChangePolynomialtemplateDegree(((PolynomialFunctionTermCurveTemplate)nextSpecification.SegmentTemplate).Degree - 1);
					else ChangeCurveSegmentCount(nextSpecification.SegmentCount - 1);
					break;
				case Key.Two:
					if (isAltDown) ChangePolynomialtemplateDegree(((PolynomialFunctionTermCurveTemplate)nextSpecification.SegmentTemplate).Degree + 1);
					else ChangeCurveSegmentCount(nextSpecification.SegmentCount + 1);
					break;
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

