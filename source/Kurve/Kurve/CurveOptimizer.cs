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
using System.Xml.Linq;
using Krach.Formats.Svg;

namespace Kurve.Component
{
	class CurveOptimizer
	{
		public const double SegmentDensity = 0.2;
		public const int CurvatureMarkersFactor = 5000;

		readonly Optimizer optimizer;
		readonly OptimizationWorker optimizationWorker;
		
		Specification specification;

		public event Action<BasicSpecification, Kurve.Curves.Curve> CurveChanged;

		public Specification Specification { get { return specification; } }
		public int SegmentCount { get { return (int)(SegmentDensity * specification.BasicSpecification.CurveLength).Ceiling(); } }

		public CurveOptimizer(OptimizationWorker optimizationWorker, Specification specification)
		{
			if (optimizationWorker == null) throw new ArgumentNullException("optimizationWorker");

			this.optimizer = new Optimizer();
			this.optimizationWorker = optimizationWorker;

			this.specification = specification;
		}

		public void Submit(BasicSpecification basicSpecification)
		{
			optimizationWorker.SubmitTask(this, curveOptimizer => curveOptimizer.Optimize(basicSpecification));
		}
		public IEnumerable<XElement> GetSvgPaths()
		{
			Kurve.Curves.Curve curve = optimizer.GetCurve(specification);
			
			XAttribute curveStyle = new XAttribute("style", "fill:none; stroke:black; stroke-width:2");
			IEnumerable<string> curveCommands = Enumerables.Concatenate
			(
				Enumerables.Create(Svg.MoveTo(curve.GetPoint(0))),
				from positions in Scalars.GetIntermediateValuesSymmetric(0, 1, SegmentCount + 1).GetRanges()
				let controlPoint1 = curve.GetPoint(positions.Item1) + (1.0 / (3 * SegmentCount)) * curve.GetVelocity(positions.Item1)
				let controlPoint2 = curve.GetPoint(positions.Item2) - (1.0 / (3 * SegmentCount)) * curve.GetVelocity(positions.Item2)
				let point2 = curve.GetPoint(positions.Item2)
				select Svg.CurveTo(controlPoint1, controlPoint2, point2)
			);
			yield return new XElement(Svg.Namespace + "path", curveStyle, new XAttribute("d", curveCommands.Separate(" ").AggregateString()));
			
			XAttribute curvatureMarkersStyle = new XAttribute("style", "fill:none; stroke:blue; stroke-width:0.5");
			IEnumerable<string> curvatureMarkersCommands =
			(
				from positions in Scalars.GetIntermediateValuesSymmetric(0, 1, SegmentCount + 1).GetRanges()
				let position = Enumerables.Average(positions.Item1, positions.Item2)
				let curvatureVector = CurvatureMarkersFactor * curve.GetCurvature(position) * curve.GetNormalVector(position)
				select Svg.Line(curve.GetPoint(position), curve.GetPoint(position) + curvatureVector)
			);
			yield return new XElement(Svg.Namespace + "path", curvatureMarkersStyle, new XAttribute("d", curvatureMarkersCommands.Separate(" ").AggregateString()));
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
				Kurve.Curves.Curve curve = new DiscreteCurve(optimizer.GetCurve(specification), (int)basicSpecification.CurveLength);
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

