using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Cairo;
using Krach.Graphics;
using Kurve.Curves;
using Kurve.Curves.Optimization;
using Kurve.Interface;
using Gtk;
using System.Xml.Linq;
using Krach.Formats.Svg;

namespace Kurve.Component
{
	class RootComponent : Component, IDisposable
	{
		readonly Window parentWindow;
		readonly OptimizationWorker optimizationWorker;
		readonly BackgroundComponent backgroundComponent;
		readonly List<CurveComponent> curveComponents;

		bool disposed = false;

		public event System.Action ComponentChanged;

		protected override IEnumerable<Component> SubComponents
		{
			get
			{
				if (backgroundComponent != null) yield return backgroundComponent;

				foreach (CurveComponent curveComponent in curveComponents) yield return curveComponent;
			}
		}

		public RootComponent(Window parentWindow, IEnumerable<string> parameters)
		{
			if (parentWindow == null) throw new ArgumentNullException("parentWindow");

			if (parameters.Count() > 1) throw new ArgumentException("parameter 'parameters' contained more than one item.");

			this.parentWindow = parentWindow;
			this.optimizationWorker = new OptimizationWorker();
			if (parameters.Count() == 1) this.backgroundComponent = new BackgroundComponent(this, parameters.Single());
			this.curveComponents = new List<CurveComponent>();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				optimizationWorker.Dispose();
			}
		}

		void AddCurve(CurveComponent curveComponent)
		{
			curveComponent.RemoveCurve += delegate()
			{
				curveComponents.Remove(curveComponent);

				Changed();
			};

			curveComponents.Add(curveComponent);
		}
		void AddCurve()
		{
			BasicSpecification basicSpecification = new BasicSpecification
			(
				100,
				1,
				new PolynomialFunctionTermCurveTemplate(10),
				Enumerables.Create
				(
            		new PointCurveSpecification(0.0, new Vector2Double(300, 300)),
            		new PointCurveSpecification(1.0, new Vector2Double(400, 300))
				)
			);

			AddCurve(new CurveComponent(this, optimizationWorker, new Specification(basicSpecification)));
		}

		public override void KeyDown(Kurve.Interface.Key key)
		{
			base.KeyDown(key);
		}
		public override void KeyUp(Kurve.Interface.Key key)
		{
			switch (key)
			{
				case Kurve.Interface.Key.N: AddCurve(); break;
				case Kurve.Interface.Key.L: Load(); break;
				case Kurve.Interface.Key.S: Save(); break;
				case Kurve.Interface.Key.E: Export(); break;
			}

			base.KeyUp(key);
		}

		public override void Changed()
		{
			if (ComponentChanged != null) ComponentChanged();
		}

		void Load()
		{
			using (FileChooserDialog fileChooser = new FileChooserDialog("Open", parentWindow, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept))
			{
				if ((ResponseType)fileChooser.Run() == ResponseType.Accept)
				{
					curveComponents.Clear();

					foreach (XElement element in XElement.Load(fileChooser.Filename).Elements())
						AddCurve(new CurveComponent(this, optimizationWorker, new Specification(element)));
				}

				fileChooser.Destroy();
			}
		}
		void Save()
		{
			using (FileChooserDialog fileChooser = new FileChooserDialog("Save", parentWindow, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept))
			{
				if ((ResponseType)fileChooser.Run() == ResponseType.Accept) 
				{
					new XElement
					(
						"curves",
						from curveComponent in curveComponents
						select curveComponent.Specification.XElement
					)
					.Save(fileChooser.Filename);
				}
				
				fileChooser.Destroy();
			}
		}
		
		void DoExport(IEnumerable<CurveOptimizer> optimizers, String filename) 
		{
			int width, height;
			parentWindow.GetSize(out width, out height);

			Svg.Document
			(
				new Orthotope2Double(0, width, 0, height),
				from optimizer in optimizers
				from path in optimizer.GetSvgPaths()
				select path
			)
			.Save(filename);
		}
		void Export() 
		{
			using (FileChooserDialog fileChooser = new FileChooserDialog("Export", parentWindow, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept))
			{
				if ((ResponseType)fileChooser.Run() == ResponseType.Accept) 
				{
					IEnumerable<CurveOptimizer> optimizers =
					(
						from curveComponent in curveComponents
						select curveComponent.CurveOptimizer
					);
					string path = fileChooser.Filename;
					
					this.optimizationWorker.SubmitTask(new CurveOptimizer(optimizationWorker, null), curveOptimizer => DoExport(optimizers, path));
				}
				
				fileChooser.Destroy();
			}
		}
	}
}

