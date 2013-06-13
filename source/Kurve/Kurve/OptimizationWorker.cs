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
		Specification currentSpecification;
		IEnumerable<Kurve.Curves.Curve> currentSegments;
		BasicSpecification nextBasicSpecification;

		public event System.Action Update;

		public IEnumerable<Kurve.Curves.Curve> CurrentSegments
		{
			get
			{
				lock (synchronization) return currentSegments;
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
			while (true)
			{
				workAvailable.WaitOne();

				Specification newSpecification;
				lock (synchronization) newSpecification = currentSpecification == null ? new Specification(nextBasicSpecification) : new Specification(nextBasicSpecification, currentSpecification.Position);

				Specification newCurrentSpecification = optimizer.Normalize(newSpecification);
				lock (synchronization) currentSpecification = newCurrentSpecification;

				IEnumerable<Kurve.Curves.Curve> newCurrentSegments = optimizer.GetSegments(newCurrentSpecification);
				lock (synchronization) currentSegments = newCurrentSegments;

				Application.Invoke(delegate (object sender, EventArgs e) { OnUpdate(); });
			}
		}
	}
}