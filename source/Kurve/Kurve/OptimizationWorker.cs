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
using Kurve.Interface;
using Kurve.Component;

namespace Kurve
{
	class OptimizationWorker : IDisposable
	{
		readonly ManualResetEvent workAvailable;
		readonly Thread workerThread;
		readonly Dictionary<CurveOptimizer, BasicSpecification> optimizationTasks;

		bool disposed = false;
		bool running = true;

		public OptimizationWorker()
		{
			this.workAvailable = new ManualResetEvent(false);
			this.workerThread = new Thread(Work);
			this.workerThread.Start();
			this.optimizationTasks = new Dictionary<CurveOptimizer, BasicSpecification>();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				running = false;
				workAvailable.Set();

				workerThread.Join();
				workAvailable.Dispose();
				
				GC.SuppressFinalize(this);
			}
		}
		public void SubmitTask(CurveOptimizer curveOptimizer, BasicSpecification basicSpecification)
		{
			lock (optimizationTasks)
			{
				optimizationTasks[curveOptimizer] = basicSpecification;

				workAvailable.Set();
			}
		}

		void Work()
		{
			while (true)
			{
				workAvailable.WaitOne();

				if (!running) break;

				KeyValuePair<CurveOptimizer, BasicSpecification> item;

				lock (optimizationTasks)
				{
					item = optimizationTasks.First();

					optimizationTasks.Remove(item.Key);

					if (!optimizationTasks.Any()) workAvailable.Reset();
				}

				item.Key.Optimize(item.Value);
			}
		}
	}
}