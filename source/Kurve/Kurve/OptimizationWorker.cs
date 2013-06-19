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
		DiscreteCurve discreteCurve;

		public event System.Action Update;

		public DiscreteCurve DiscreteCurve { get { lock (synchronization) return discreteCurve; } }

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

				Kurve.Curves.Curve curve = optimizer.GetCurve(specification);

				lock (synchronization) discreteCurve = new Kurve.DiscreteCurve(curve);

				Application.Invoke(delegate (object sender, EventArgs e) { OnUpdate(); });
			}
		}
	}
}