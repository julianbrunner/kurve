using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Cairo;
using Kurve.Curves.Optimization;
using Kurve.Curves;
using System.Threading;
using Gtk;

namespace Kurve
{
	class OptimizationWorker : IDisposable
	{
		readonly object synchronization = new object();
		readonly Optimizer optimizer;
		readonly AutoResetEvent workAvailable;
		readonly Thread workerThread;

		bool disposed = false;
		BasicSpecification nextBasicSpecification;
		IEnumerable<IEnumerable<Vector2Double>> segmentPolygons;

		public event System.Action Update;

		public IEnumerable<IEnumerable<Vector2Double>> SegmentPolygons
		{
			get
			{
				lock (synchronization) return segmentPolygons;
			}
		}

		public OptimizationWorker()
		{
			this.optimizer = new Optimizer();
			this.workAvailable = new AutoResetEvent(false);
			this.workerThread = new Thread(Work);
			this.workerThread.Start();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				workerThread.Join();
				workAvailable.Dispose();
				
				GC.SuppressFinalize(this);
			}
		}
		public void SubmitSpecification(BasicSpecification basicSpecification)
		{
			lock (synchronization) nextBasicSpecification = basicSpecification;

			workAvailable.Set();
		}

		protected void OnUpdate()
		{
			if (Update != null) Update();
		}

		void Work()
		{
			Specification specification = null;

			while (true)
			{
				workAvailable.WaitOne();

				lock (synchronization) specification = specification == null ? new Specification(nextBasicSpecification) : new Specification(nextBasicSpecification, specification.Position);

				specification = optimizer.Normalize(specification);

				IEnumerable<Kurve.Curves.Curve> segments = optimizer.GetSegments(specification);

				lock (synchronization) segmentPolygons = GetSegmentPolygons(segments);

				Application.Invoke(delegate (object sender, EventArgs e) { OnUpdate(); });
			}
		}

		static IEnumerable<IEnumerable<Vector2Double>> GetSegmentPolygons(IEnumerable<Kurve.Curves.Curve> segments)
		{
			return
			(
				from segment in segments
				select
				(
					from position in Scalars.GetIntermediateValues(0, 1, 100)
					let point = segment.Point.Apply(Terms.Constant(position)).Evaluate()
					select new Vector2Double(point.ElementAt(0), point.ElementAt(1))
				)
				.ToArray()
			)
			.ToArray();
		}
	}
}