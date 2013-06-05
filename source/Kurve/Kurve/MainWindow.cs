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

public partial class MainWindow : Gtk.Window
{
	readonly List<Component> components = new List<Component>();

	public MainWindow(): base (Gtk.WindowType.Toplevel)
	{
		Build();

		AddComponent(new PointComponent(new Vector2Double(5, 5)));
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
			double curveLength = 4;
			int segmentCount = 10;
			CurveTemplate segmentTemplate = new PolynomialCurveTemplate(10);
			IEnumerable<CurveSpecification> curveSpecifications = Enumerables.Create<CurveSpecification>
			(
				new PointCurveSpecification(0.0, new Vector2Double(-1.0,  0.0)),
				new VelocityCurveSpecification(0.0, new Vector2Double( 0.0, -4.0)),

				new PointCurveSpecification(1.0, new Vector2Double(+1.0,  0.0)),
				new VelocityCurveSpecification(1.0, new Vector2Double( 0.0, +4.0))
			);

			BasicSpecification basicSpecification = new BasicSpecification(curveLength, segmentCount, segmentTemplate, curveSpecifications);

			Optimizer optimizer = Optimizer.Create(basicSpecification);

			IEnumerable<Kurve.Curves.Curve> result = optimizer.GetCurves();

			double segmentLength = basicSpecification.CurveLength / basicSpecification.SegmentCount;
			ValueTerm position = Terms.Variable("t");
			ValueTerm point = Terms.Variable("point", 2);
			FunctionTerm pointScaling = Terms.Scaling(Terms.Constant(1.0), point).Abstract(point);
			FunctionTerm velocityScaling = Terms.Scaling(Terms.Constant(1.0 / segmentLength), point).Abstract(point);
			FunctionTerm accelerationScaling = Terms.Scaling(Terms.Constant(0.2 / segmentLength.Square()), point).Abstract(point);

			context.LineWidth = 3;
			context.LineCap = LineCap.Butt;

			foreach (Kurve.Curves.Curve curve in result)
			{
				DrawParametricCurve(context, accelerationScaling.Apply(curve.Acceleration.Apply(position)).Abstract(position), Krach.Graphics.Colors.Cyan, Krach.Graphics.Colors.Blue);
				DrawParametricCurve(context, velocityScaling.Apply(curve.Velocity.Apply(position)).Abstract(position), Krach.Graphics.Colors.Red, Krach.Graphics.Colors.Yellow);
				DrawParametricCurve(context, pointScaling.Apply(curve.Point.Apply(position)).Abstract(position), Krach.Graphics.Colors.Red, Krach.Graphics.Colors.Blue);
			}
			
			context.LineWidth = 5;
			context.LineCap = LineCap.Round;

			DrawCurveSpecifications(context, basicSpecification.CurveSpecifications);

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

	static Vector2Double TransformPoint(Vector2Double point)
	{
		return 256 * point + new Vector2Double(384, 384);
	}
	static Cairo.Color ToCairoColor(Krach.Graphics.Color color)
	{
		return new Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha);
	}
	static void DrawPoint(Context context, Vector2Double point, Krach.Graphics.Color color)
	{
		point = TransformPoint(point);

		context.MoveTo(point.X, point.Y);
		context.LineTo(point.X, point.Y);

		context.Color = ToCairoColor(color);
		context.Stroke();
	}
	static void DrawLine(Context context, Vector2Double startPoint, Vector2Double endPoint, Krach.Graphics.Color color)
	{
		startPoint = TransformPoint(startPoint);
		endPoint = TransformPoint(endPoint);

		context.MoveTo(startPoint.X, startPoint.Y);
		context.LineTo(endPoint.X, endPoint.Y);

		context.Color = ToCairoColor(color);
		context.Stroke();
	}
	static void DrawCurveSpecifications(Context context, IEnumerable<CurveSpecification> curveSpecifications)
	{
		foreach (CurveSpecification curveSpecification in curveSpecifications)
		{
			if (curveSpecification is PointCurveSpecification)
			{
				PointCurveSpecification pointCurveSpecification = (PointCurveSpecification)curveSpecification;

				DrawPoint(context, pointCurveSpecification.Point, Krach.Graphics.Colors.Black);
			}
		}
	}
	static void DrawParametricCurve(Context context, FunctionTerm curve, Krach.Graphics.Color startColor, Krach.Graphics.Color endColor)
	{
		double stepLength = 0.01;

		for (double position = 0; position < 1; position += stepLength)
		{
			Krach.Graphics.Color color = Krach.Graphics.Color.InterpolateHsv(startColor, endColor, Scalars.InterpolateLinear, position);

			DrawLine(context, EvaluatePoint(curve, position), EvaluatePoint(curve, position + stepLength), color);
		}
	}
	static Vector2Double EvaluatePoint(FunctionTerm curve, double position)
	{
		IEnumerable<double> result = curve.Apply(Terms.Constant(position)).Evaluate();

		return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
	}
}