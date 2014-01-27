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
using Gtk;

namespace Kurve.Component
{
	class CurveOptimizer
	{
		readonly Optimizer optimizer;
		readonly OptimizationWorker optimizationWorker;
		
		Specification specification;

		public event Action<BasicSpecification, Kurve.Curves.Curve> CurveChanged;

		public Specification Specification { get { return specification; } }

		public CurveOptimizer(OptimizationWorker optimizationWorker, Specification specification)
		{
			if (optimizationWorker == null) throw new ArgumentNullException("optimizationWorker");

			this.optimizer = new Optimizer();
			this.optimizationWorker = optimizationWorker;

			this.specification = specification;
		}

		public void Submit(BasicSpecification basicSpecification)
		{
			optimizationWorker.SubmitTask(this, () => this.Optimize(basicSpecification));
		}
		public String GetSvgAttributeString(int numberOfSegments) 
		{
			Kurve.Curves.Curve curve = optimizer.GetCurve(specification);
			IEnumerable<Tuple<double, double>> ranges = Scalars.GetIntermediateValuesSymmetric(0, 1, numberOfSegments).GetRanges();
			
			string svgString = "M"+PointToSvgString(curve.GetPoint(ranges.First().Item1));
			string svgComponents = 
			(
				from range in ranges
				let controlPoint2 = curve.GetPoint(range.Item1) + (1.0 / (3 * numberOfSegments)) * Velocity(curve.GetDirection(range.Item1), curve.GetSpeed(range.Item1))
				let controlPoint3 = curve.GetPoint(range.Item2) - (1.0 / (3 * numberOfSegments)) * Velocity(curve.GetDirection(range.Item2), curve.GetSpeed(range.Item2))
				let controlPoint4 = curve.GetPoint(range.Item2)
				select " C"+PointToSvgString(controlPoint2)+" "+PointToSvgString(controlPoint3)+" "+PointToSvgString(controlPoint4)
			)
			.AggregateString();
			
			return svgString+" "+svgComponents;
		}
		
		public String GetCurvatureIndicators(int count) {
			Kurve.Curves.Curve curve = optimizer.GetCurve(specification);
			
			return (
				from positions in Scalars.GetIntermediateValuesSymmetric(0, 1, 250).GetRanges()
				let point = 0.5 * (curve.GetPoint(positions.Item1) + curve.GetPoint(positions.Item2))
				let direction = curve.GetDirection((positions.Item1 + positions.Item2) / 2)
				let directionVector = new Vector2Double(Scalars.Cosine(direction), Scalars.Sine(direction))
				let angularDirection = new Vector2Double(directionVector.Y, -directionVector.X)
				let curvature = curve.GetCurvature((positions.Item1 + positions.Item2) / 2)
				let curvatureVector = 10000 * curvature * angularDirection
				select "M "+PointToSvgString(point)+" "+PointToSvgString(point + curvatureVector)+" "
			)
			.AggregateString();
		}
		
		String PointToSvgString(Vector2Double point) 
		{
			return point.X+","+point.Y;
		}
		Vector2Double Velocity(double direction, double speed) 
		{
			return new Vector2Double(Math.Cos(direction), Math.Sin(direction)) * speed;
		}
		
		void Optimize(BasicSpecification basicSpecification)
		{
			if (specification == null) specification = new Specification(basicSpecification);

			// TODO: make sure the new curve resembles the old one, even when SegmentCount and/or SegmentTemplate are changed
			if (basicSpecification.SegmentCount != specification.BasicSpecification.SegmentCount || basicSpecification.SegmentTemplate != specification.BasicSpecification.SegmentTemplate) specification = new Specification(basicSpecification);

			specification = new Specification(basicSpecification, specification.Position);

			try
			{
				Stopwatch stopwatch = new Stopwatch();

				stopwatch.Restart();
				specification = optimizer.Normalize(specification);
				stopwatch.Stop();

				Console.WriteLine("normalization: {0} s", stopwatch.Elapsed.TotalSeconds);

				stopwatch.Restart();
				Kurve.Curves.Curve curve = new DiscreteCurve(optimizer.GetCurve(specification));
				stopwatch.Stop();

				Console.WriteLine("discrete curve: {0} s", stopwatch.Elapsed.TotalSeconds);

				Application.Invoke
				(
					delegate (object sender, EventArgs e)
					{
						if (CurveChanged != null) CurveChanged(basicSpecification, curve);
					}
				);
			}
			catch (InvalidOperationException exception)
			{
				Terminal.Write(exception.Message, ConsoleColor.Red);
				Terminal.WriteLine();
			}
		}
	}
}

