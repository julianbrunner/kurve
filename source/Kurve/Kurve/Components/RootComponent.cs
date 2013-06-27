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
using Kurve.Interface;

namespace Kurve.Component
{
	class RootComponent : Component, IDisposable
	{
		readonly OptimizationWorker optimizationWorker;
		readonly List<CurveComponent> curveComponents;

		bool disposed = false;

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
		}

		public void AddCurve()
		{
			BasicSpecification basicSpecification = new BasicSpecification
			(
				100,
			    2,
				new PolynomialFunctionTermCurveTemplate(10),
				Enumerables.Create(
            		new PointCurveSpecification(0.0, new Vector2Double(300, 300)),
            		new PointCurveSpecification(1.0, new Vector2Double(400, 300))
				)
			);

			this.curveComponents.Add(new CurveComponent(this, optimizationWorker, new Specification(basicSpecification)));
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				optimizationWorker.Dispose();
			}
		}

		public override void KeyUp(Key key)
		{
			if (key == Key.Alt) {
				AddCurve();
			}

			base.KeyDown(key);
		}

		public override void Changed()
		{
			if (ComponentChanged != null) ComponentChanged();
		}
	}
}

