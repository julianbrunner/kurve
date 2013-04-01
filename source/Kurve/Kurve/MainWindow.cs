using System;
using Gtk;
using Gdk;
using Cairo;
using Kurve.Curves;
using Krach.Basics;
using System.Linq;
using Krach.Extensions;
using System.Collections.Generic;
using Krach.Calculus.Terms;
using Kurve.Curves.Specification;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow(): base (Gtk.WindowType.Toplevel)
	{
		Build();
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();

		a.RetVal = true;
	}
	protected void OnDrawingarea1ExposeEvent(object o, ExposeEventArgs args)
	{
		using (Context context = CairoHelper.Create(drawingarea1.GdkWindow))
		{
			PointCurveSpecification point1 = new PointCurveSpecification(0.2, new Vector2Double(400, 400));
			PointCurveSpecification point2 = new PointCurveSpecification(0.3, new Vector2Double(500, 500));
			PointCurveSpecification point3 = new PointCurveSpecification(0.9, new Vector2Double(600, 400));
			Kurve.Curves.Curve segmentCurve = Kurve.Curves.Curve.CreatePolynomialCurve(2);
			
			Optimizer optimizer = new Optimizer(Enumerables.Create(point1, point2, point3), segmentCurve, 1);

			IEnumerable<Kurve.Curves.Curve> curves = optimizer.Optimize();
			
			context.LineWidth = 5;
			context.LineCap = LineCap.Round;

			foreach (Kurve.Curves.Curve curve in curves)
				DrawParametricCurve(context, curve, new Cairo.Color(0, 0, 1));

			DrawPoint(context, point1.Point, new Cairo.Color(1, 0, 0));
			DrawPoint(context, point2.Point, new Cairo.Color(1, 0, 0));
			DrawPoint(context, point3.Point, new Cairo.Color(1, 0, 0));
			
			context.Target.Dispose();
		}
	}

	static void DrawPoint(Context context, Vector2Double position, Cairo.Color color)
	{
		context.MoveTo(position.X, position.Y);
		context.LineTo(position.X, position.Y);

		context.Color = color;
		context.Stroke();
	}
	static void DrawParametricCurve(Context context, Kurve.Curves.Curve curve, Cairo.Color color)
	{
		Vector2Double startPoint = curve.EvaluatePoint(0);
		context.MoveTo(startPoint.X, startPoint.Y);

		for (double position = 0; position <= 1; position += 0.001)
		{
			Vector2Double point = curve.EvaluatePoint(position);
			context.LineTo(point.X, point.Y);
		}
		
		context.Color = color;
		context.Stroke();
	}
}
