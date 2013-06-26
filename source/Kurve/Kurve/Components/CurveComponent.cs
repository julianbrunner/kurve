using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Cairo;
using Krach.Graphics;
using Kurve.Curves;
using Kurve.Curves.Optimization;
using Gtk;
using Krach.Maps.Abstract;
using System.Diagnostics;
using Krach.Maps.Scalar;
using Krach.Maps;
using Kurve.Interface;

namespace Kurve.Component
{
	class CurveComponent : Component
	{
		readonly Optimizer optimizer;
		readonly OptimizationWorker optimizationWorker;

		Specification specification;
		DiscreteCurve discreteCurve;		

		double curveLength;
		int segmentCount;
		FunctionTermCurveTemplate segmentTemplate;
		IEnumerable<PointSpecificationComponent> pointSpecificationComponents;
		IEnumerable<InterSpecificationComponent> interSpecificationComponents;

		protected override IEnumerable<Component> SubComponents
		{
			get
			{
				return Enumerables.Concatenate<CurveControlComponent>
				(
					pointSpecificationComponents,
					interSpecificationComponents
				);
			}
		}

		public CurveComponent (Component parent, OptimizationWorker optimizationWorker) : base(parent)
		{
			if (optimizationWorker == null)
				throw new ArgumentNullException ("optimizationWorker");

			this.optimizer = new Optimizer ();
			this.optimizationWorker = optimizationWorker;

			this.curveLength = 1000;
			this.segmentCount = 1;
			this.segmentTemplate = new PolynomialFunctionTermCurveTemplate (10);
			this.pointSpecificationComponents = Enumerables.Create
			(
				new PointSpecificationComponent (this, 0.0, new Vector2Double (100, 100)),
				new PointSpecificationComponent (this, 0.2, new Vector2Double (200, 200)),
				new PointSpecificationComponent (this, 0.4, new Vector2Double (300, 300)),
				new PointSpecificationComponent (this, 0.6, new Vector2Double (400, 400)),
				new PointSpecificationComponent (this, 0.8, new Vector2Double (500, 500)),
				new PointSpecificationComponent (this, 1.0, new Vector2Double (600, 600))
			)
			.ToArray ();

			this.interSpecificationComponents = pointSpecificationComponents.GetRanges().Select(specificationPair => new InterSpecificationComponent(this, specificationPair.Item1, specificationPair.Item2)).ToArray();
			
			foreach (PointSpecificationComponent pointSpecificationComponent in pointSpecificationComponents) pointSpecificationComponent.SpecificationChanged += SpecificationChanged;

			foreach (CurveControlComponent controlComponent in SubComponents) controlComponent.InsertLength += InsertLength;

			SpecificationChanged();
		}

		void InsertLength(double position, double length)
		{
			double shiftFactor = curveLength / (curveLength + length);
			curveLength += length;

			foreach (SpecificationComponent component in pointSpecificationComponents) 
			{

				component.setPosition(MovePosition(component.Position, position, shiftFactor));
				
				Console.WriteLine ("position: {0}", component.Position);

				Changed();
				SpecificationChanged();
			}
		}

		static double MovePosition(double position, double insertPosition, double shiftFactor)
		{
			if (position == insertPosition) return position;
			if (position < insertPosition) return position * shiftFactor;
			if (position > insertPosition) return 1 - (1 - position) * shiftFactor;

			throw new InvalidOperationException();
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
			DiscreteCurve newDiscreteCurve = new DiscreteCurve(optimizer.GetCurve(specification));
			stopwatch.Stop();

			Console.WriteLine("discrete curve: {0} s", stopwatch.Elapsed.TotalSeconds);

			Application.Invoke
			(
				delegate (object sender, EventArgs e)
				{
					discreteCurve = newDiscreteCurve;

					foreach (InterSpecificationComponent component in interSpecificationComponents) component.DiscreteCurve = discreteCurve;

					Changed();
				}
			);
		}

		public override void Draw(Context context)
		{
			if (discreteCurve == null) return;

			foreach (Tuple<double, double> positions in Scalars.GetIntermediateValues(0, 1, 100).GetRanges()) 
			{
				double stretchFactor = discreteCurve.GetVelocity((positions.Item1 + positions.Item2) / 2).Length / specification.BasicSpecification.CurveLength;

				Krach.Graphics.Color color = Colors.Green;
				if (stretchFactor < 1) color = Krach.Graphics.Color.InterpolateHsv(Colors.Blue, Colors.Green, Scalars.InterpolateLinear, (1.0 * stretchFactor).Clamp(0, 1));
				if (stretchFactor > 1) color = Krach.Graphics.Color.InterpolateHsv(Colors.Red, Colors.Green, Scalars.InterpolateLinear, (1.0 / stretchFactor).Clamp(0, 1));

				InterfaceUtility.DrawLine(context, discreteCurve.GetPoint(positions.Item1), discreteCurve.GetPoint(positions.Item2), 2, color);
			}

			base.Draw(context);
		}
		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (!pointSpecificationComponents.Any(specificationPoint => specificationPoint.Selected))
			{
				switch (scrollDirection)
				{
					case Kurve.Interface.ScrollDirection.Up: curveLength -= 10; break;
					case Kurve.Interface.ScrollDirection.Down: curveLength += 10; break;
					default: throw new ArgumentException();
				}

				SpecificationChanged();
			}

			base.Scroll(scrollDirection);
		}

		void SpecificationChanged()
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
	}
}

