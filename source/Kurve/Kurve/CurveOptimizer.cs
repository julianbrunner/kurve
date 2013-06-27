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

namespace Kurve.Component
{
	class CurveOptimizer
	{
		readonly Optimizer optimizer;
		readonly OptimizationWorker optimizationWorker;
		
		Specification specification;

		public event Action<BasicSpecification, Kurve.Curves.Curve> CurveChanged;

		public CurveOptimizer(OptimizationWorker optimizationWorker, Specification specification)
		{
			if (optimizationWorker == null) throw new ArgumentNullException("optimizationWorker");

			this.optimizer = new Optimizer();
			this.optimizationWorker = optimizationWorker;
			this.specification = specification;
		}

		public void Submit(BasicSpecification basicSpecification)
		{
			optimizationWorker.SubmitTask(this, basicSpecification);
		}
		public void Optimize(BasicSpecification basicSpecification)
		{
			if (specification == null) specification = new Specification(basicSpecification);
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

