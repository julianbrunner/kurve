using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Krach.Graphics;
using Kurve.Curves;
using Kurve.Curves.Optimization;
using Krach.Maps.Abstract;
using System.Diagnostics;
using Krach.Maps.Scalar;
using Krach.Maps;
using Kurve.Interface;
using Gtk;
using Cairo;

namespace Kurve.Component
{
	class CurveComponent : Component
	{
		readonly Optimizer optimizer;
		readonly OptimizationWorker optimizationWorker;

		Specification specification;
		Kurve.Curves.Curve curve;		

		double curveLength;
		int segmentCount;
		FunctionTermCurveTemplate segmentTemplate;
		IEnumerable<PointSpecificationComponent> pointSpecificationComponents;

		CurveLengthComponent curveLengthComponent;
		IEnumerable<InterSpecificationComponent> interSpecificationComponents;

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

		protected override IEnumerable<Component> SubComponents
		{
			get
			{
				return Enumerables.Concatenate<Component>
				(
					pointSpecificationComponents,
					Enumerables.Create(curveLengthComponent),
					interSpecificationComponents
				);
			}
		}

		public CurveComponent(Component parent, OptimizationWorker optimizationWorker) : base(parent)
		{
			if (optimizationWorker == null) throw new ArgumentNullException("optimizationWorker");

			this.optimizer = new Optimizer();
			this.optimizationWorker = optimizationWorker;

			this.curveLength = 1000;
			this.segmentCount = 1;
			this.segmentTemplate = new PolynomialFunctionTermCurveTemplate(10);
			this.pointSpecificationComponents = Enumerables.Create
			(
				new PointSpecificationComponent(this, 0.0, new Vector2Double(100, 100)),
				new PointSpecificationComponent(this, 0.2, new Vector2Double(200, 200)),
				new PointSpecificationComponent(this, 0.4, new Vector2Double(300, 300)),
				new PointSpecificationComponent(this, 0.6, new Vector2Double(400, 400)),
				new PointSpecificationComponent(this, 0.8, new Vector2Double(500, 500)),
				new PointSpecificationComponent(this, 1.0, new Vector2Double(600, 600))
			)
			.ToArray();

			this.curveLengthComponent = new CurveLengthComponent(this);
			this.interSpecificationComponents =
			(
				from specificationRange in pointSpecificationComponents.GetRanges()
				select new InterSpecificationComponent(this, specificationRange.Item1, specificationRange.Item2)
			)
			.ToArray();

			foreach (PointSpecificationComponent pointSpecificationComponent in pointSpecificationComponents) pointSpecificationComponent.SpecificationChanged += SpecificationChanged;

			curveLengthComponent.InsertLength += InsertLength;

			foreach (PositionedControlComponent positionedControlComponent in PositionedControlComponents) positionedControlComponent.InsertLength += InsertLength;

			RebuildSpecification();
		}

		void SpecificationChanged()
		{
			RebuildSpecification();
		}
		void InsertLength(double length)
		{
			if (PositionedControlComponents.Any(positionedControlComponent => positionedControlComponent.Selected)) return;

			curveLength += length;

			RebuildSpecification();
		}
		void InsertLength(double position, double length)
		{
			double newCurveLength = curveLength + length;
			double lengthRatio = curveLength / newCurveLength;

			curveLength = newCurveLength;

			foreach (SpecificationComponent specificationComponent in SpecificationComponents)
				specificationComponent.CurrentPosition = ShiftPosition(specificationComponent.CurrentPosition, position, lengthRatio);

			RebuildSpecification();
		}

		public void Optimize(BasicSpecification basicSpecification)
		{
			if (specification == null) specification = new Specification(basicSpecification);
			if (basicSpecification.SegmentCount != specification.BasicSpecification.SegmentCount || basicSpecification.SegmentTemplate != specification.BasicSpecification.SegmentTemplate) specification = new Specification(basicSpecification);

			specification = new Specification(basicSpecification, specification.Position);

			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Restart();
			specification = optimizer.Normalize(specification);
			stopwatch.Stop();

			Console.WriteLine("normalization: {0} s", stopwatch.Elapsed.TotalSeconds);

			stopwatch.Restart();
			Kurve.Curves.Curve newCurve = new DiscreteCurve(optimizer.GetCurve(specification));
			stopwatch.Stop();

			Console.WriteLine("discrete curve: {0} s", stopwatch.Elapsed.TotalSeconds);

			Application.Invoke
			(
				delegate (object sender, EventArgs e)
				{
					curve = newCurve;

					foreach (PositionedControlComponent positionedControlComponent in PositionedControlComponents) positionedControlComponent.Curve = curve;

					Changed();
				}
			);
		}

		public override void Draw(Context context)
		{
			if (curve == null) return;

			foreach (Tuple<double, double> positions in Scalars.GetIntermediateValues(0, 1, 100).GetRanges()) 
			{
				double stretchFactor = curve.GetVelocity((positions.Item1 + positions.Item2) / 2).Length / specification.BasicSpecification.CurveLength;

				Krach.Graphics.Color color = Colors.Green;
				if (stretchFactor < 1) color = Krach.Graphics.Color.InterpolateHsv(Colors.Blue, Colors.Green, Scalars.InterpolateLinear, (1.0 * stretchFactor).Clamp(0, 1));
				if (stretchFactor > 1) color = Krach.Graphics.Color.InterpolateHsv(Colors.Red, Colors.Green, Scalars.InterpolateLinear, (1.0 / stretchFactor).Clamp(0, 1));

				InterfaceUtility.DrawLine(context, curve.GetPoint(positions.Item1), curve.GetPoint(positions.Item2), 2, color);
			}

			base.Draw(context);
		}

		void RebuildSpecification()
		{
			optimizationWorker.SubmitTask
			(
				this,
				new BasicSpecification
				(
					curveLength,
					segmentCount,
					segmentTemplate,
					(
						from pointSpecificationComponent in pointSpecificationComponents
						select new PointCurveSpecification(pointSpecificationComponent.Position, pointSpecificationComponent.Point)
					)
					.ToArray()
				)
			);
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

