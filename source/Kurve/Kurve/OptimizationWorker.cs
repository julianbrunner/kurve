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

namespace Kurve
{
	class OptimizationWorker : IDisposable
	{
		readonly ManualResetEvent workAvailable;
		readonly Thread workerThread;
		readonly Dictionary<CurveComponent, BasicSpecification> optimizationTasks;

		bool disposed = false;

		public OptimizationWorker()
		{
			this.workAvailable = new ManualResetEvent(false);
			this.workerThread = new Thread(Work);
			this.workerThread.Start();
			this.optimizationTasks = new Dictionary<CurveComponent, BasicSpecification>();
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
		public void SubmitTask(CurveComponent curveComponent, BasicSpecification basicSpecification)
		{
			lock (optimizationTasks)
			{
				optimizationTasks[curveComponent] = basicSpecification;

				workAvailable.Set();
			}
		}

		void Work()
		{
			while (true)
			{
				workAvailable.WaitOne();

				KeyValuePair<CurveComponent, BasicSpecification> item;

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