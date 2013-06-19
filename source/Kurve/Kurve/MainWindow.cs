using System;
using Gtk;
using Gdk;
using Cairo;
using Kurve.Curves;
using Krach.Basics;
using System.Linq;
using Krach.Extensions;
using System.Collections.Generic;
using Wrappers.Casadi;
using System.Xml.Linq;
using Kurve.Interface;
using Kurve.Curves.Optimization;
using Kurve;

public partial class MainWindow : Gtk.Window
{
	readonly OptimizationWorker worker;

	double curveLength = 1000;
	int segmentCount = 1;
	FunctionTermCurveTemplate segmentTemplate = new PolynomialFunctionTermCurveTemplate(10);
	
	CurveComponent curveComponent;
	IEnumerable<PointSpecificationComponent> pointSpecificationComponents;

	IEnumerable<Component> Components
	{
		get
		{
			return Enumerables.Concatenate<Component>
			(
				Enumerables.Create(curveComponent),
				pointSpecificationComponents
			);
		}
	}

	public MainWindow(): base (Gtk.WindowType.Toplevel)
	{
		Build();

		this.worker = new OptimizationWorker();
		this.worker.Update += WorkerUpdate;

		this.curveComponent = new CurveComponent();
		this.curveComponent.Update += Invalidate;

		this.pointSpecificationComponents = Enumerables.Create
		(
			new PointSpecificationComponent(0.0, new Vector2Double(100, 100)),
			new PointSpecificationComponent(0.2, new Vector2Double(200, 200)),
			new PointSpecificationComponent(0.4, new Vector2Double(300, 300)),
			new PointSpecificationComponent(0.6, new Vector2Double(400, 400)),
			new PointSpecificationComponent(0.8, new Vector2Double(500, 500)),
			new PointSpecificationComponent(1.0, new Vector2Double(600, 600))
		)
		.ToArray();

		foreach (PointSpecificationComponent specificationPoint in pointSpecificationComponents)
		{
			specificationPoint.Update += Invalidate;
			specificationPoint.Update += UpdateSpecification;
		}

		UpdateSpecification();
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();

		a.RetVal = true;
	}
	protected void OnExposeEvent(object o, ExposeEventArgs args)
	{
		using (Context context = CairoHelper.Create(GdkWindow))
		{
			foreach (Component component in Components) component.Draw(context);

			context.Target.Dispose();
		}
	}
	protected void OnButtonPressEvent(object o, ButtonPressEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);
		MouseButton button = (MouseButton)args.Event.Button;

		foreach (Component component in Components) component.MouseDown(position, button);
	}
	protected void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);
		MouseButton button = (MouseButton)args.Event.Button;

		foreach (Component component in Components) component.MouseUp(position, button);
	}
	protected void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);

		foreach (Component component in Components) component.MouseMove(position);
	}
	protected void OnScrollEvent(object o, ScrollEventArgs args)
	{
		Kurve.Interface.ScrollDirection scrollDirection;

		switch (args.Event.Direction)
		{
			case Gdk.ScrollDirection.Up: scrollDirection = Kurve.Interface.ScrollDirection.Up; break;
			case Gdk.ScrollDirection.Down: scrollDirection = Kurve.Interface.ScrollDirection.Down; break;
			default: return;
		}

		foreach (Component component in Components) component.Scroll(scrollDirection);

		if (!pointSpecificationComponents.Any(specificationPoint => specificationPoint.Selected))
		{
			switch (scrollDirection)
			{
				case Kurve.Interface.ScrollDirection.Up: curveLength -= 10; break;
				case Kurve.Interface.ScrollDirection.Down: curveLength += 10; break;
				default: throw new ArgumentException();
			}

			UpdateSpecification();
		}
	}

	void Invalidate()
	{
		GdkWindow.InvalidateRegion(GdkWindow.VisibleRegion, true);
	}

	void UpdateSpecification()
	{
		worker.SubmitSpecification
		(
			new BasicSpecification
			(
				curveLength,
				segmentCount,
				segmentTemplate,
				(
					from point in pointSpecificationComponents
					select new PointCurveSpecification(point.Position, point.Point)
				)
				.ToArray()
			)
		);
	}
	void WorkerUpdate()
	{
		curveComponent.DiscreteCurve = worker.DiscreteCurve;
	}
}