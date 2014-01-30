using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Cairo;
using Kurve.Curves.Optimization;
using Kurve.Curves;
using System.Threading;
using Kurve.Interface;
using Kurve.Component;

namespace Kurve
{
	class OptimizationWorker : IDisposable
	{
		readonly ManualResetEvent workAvailable;
		readonly Thread workerThread;
		readonly Dictionary<CurveOptimizer, Action<CurveOptimizer>> optimizationTasks;

		bool disposed = false;
		bool running = true;

		public OptimizationWorker()
		{
			this.workAvailable = new ManualResetEvent(false);
			this.workerThread = new Thread(Work);
			this.workerThread.Start();
			this.optimizationTasks = new Dictionary<CurveOptimizer, Action<CurveOptimizer>>();
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
		public void SubmitTask(CurveOptimizer curveOptimizer, Action<CurveOptimizer> action)
		{
			lock (optimizationTasks)
			{
				optimizationTasks[curveOptimizer] = action;

				workAvailable.Set();
			}
		}

		void Work()
		{
			while (true)
			{
				workAvailable.WaitOne();

				if (!running) break;

				KeyValuePair<CurveOptimizer, Action<CurveOptimizer>> task;

				lock (optimizationTasks)
				{
					task = optimizationTasks.First();
					
					optimizationTasks.Remove(task.Key);
					
					if (!optimizationTasks.Any()) workAvailable.Reset();
				}

				task.Value(task.Key);
			}
		}
	}
}