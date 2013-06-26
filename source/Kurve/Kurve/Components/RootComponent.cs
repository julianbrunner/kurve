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

namespace Kurve.Component
{
	class RootComponent : Component
	{
		readonly OptimizationWorker optimizationWorker;
		readonly List<CurveComponent> curveComponents;

		public event Action ComponentChanged;

		protected override IEnumerable<Component> SubComponents
		{
			get
			{
				return curveComponents;
			}
		}

		public RootComponent()
		{
			this.optimizationWorker = new OptimizationWorker();
			this.curveComponents = new List<CurveComponent>();

			this.curveComponents.Add(new CurveComponent(this, optimizationWorker));
		}

		public override void Changed()
		{
			if (ComponentChanged != null) ComponentChanged();
		}
	}
}

