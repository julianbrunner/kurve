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
	readonly List<Component> components;
	readonly OptimizationWorker worker;
	readonly CurveComponent curve;
	readonly IEnumerable<PointComponent> points;

	public MainWindow(): base (Gtk.WindowType.Toplevel)
	{
		Build();

		this.components = new List<Component>();
		this.worker = new OptimizationWorker();
		this.worker.Update += WorkerUpdate;
		this.curve = new CurveComponent();
		this.points = Enumerables.Create
		(
			new PointComponent(0.0, new Vector2Double(0, 0)),
			new PointComponent(0.5, new Vector2Double(50, 50)),
			new PointComponent(1.0, new Vector2Double(100, 100))
		)
		.ToArray();

		AddComponent(curve);
		foreach (PointComponent point in points)
		{
			AddComponent(point);

			point.Update += UpdateSpecification;
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
			foreach (Component component in components) component.Draw(context);

			context.Target.Dispose();
		}
	}
	protected void OnButtonPressEvent(object o, ButtonPressEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);
		MouseButton button = (MouseButton)args.Event.Button;

		foreach (Component component in components) component.MouseDown(position, button);
	}
	protected void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);
		MouseButton button = (MouseButton)args.Event.Button;

		foreach (Component component in components) component.MouseUp(position, button);
	}
	protected void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);

		foreach (Component component in components) component.MouseMove(position);
	}
	protected void OnScrollEvent(object o, ScrollEventArgs args)
	{
		Kurve.Interface.ScrollDirection direction;

		switch (args.Event.Direction)
		{
			case Gdk.ScrollDirection.Up: direction = Kurve.Interface.ScrollDirection.Up; break;
			case Gdk.ScrollDirection.Down: direction = Kurve.Interface.ScrollDirection.Down; break;
			default: return;
		}

		foreach (Component component in components) component.Scroll(direction);
	}

	void Invalidate()
	{
		GdkWindow.InvalidateRegion(GdkWindow.VisibleRegion, true);
	}
	void AddComponent(Component component)
	{
		component.Update += Invalidate;

		components.Add(component);
	}
	void RemoveComponent(Component component)
	{
		component.Update -= Invalidate;

		components.Remove(component);
	}
	void UpdateSpecification()
	{
		double curveLength = 200;
		int segmentCount = 10;
		CurveTemplate segmentTemplate = new PolynomialCurveTemplate(10);
		IEnumerable<CurveSpecification> curveSpecifications =
		(
			from point in points
			select new PointCurveSpecification(point.Position, point.Point)
		)
		.ToArray();
		BasicSpecification basicSpecification = new BasicSpecification(curveLength, segmentCount, segmentTemplate, curveSpecifications);

		worker.SubmitSpecification(basicSpecification);
	}
	void WorkerUpdate()
	{
		curve.Segments = worker.CurrentSegments;
	}
}