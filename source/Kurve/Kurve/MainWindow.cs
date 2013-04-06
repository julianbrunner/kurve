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
			IEnumerable<CurveSpecification> curveSpecifications = Enumerables.Create<CurveSpecification>
			(
				new PointCurveSpecification(0.0, new Vector2Double(0.0, 0.0)),
				new VelocityCurveSpecification(0.2, new Vector2Double(+0.5, +0.5)),
				new VelocityCurveSpecification(0.5, new Vector2Double(+0.5,  0.0)),
				new VelocityCurveSpecification(0.9, new Vector2Double(+0.5, +0.5))
			);
			CurveTemplate segmentCurveTemplate = CurveTemplate.CreatePolynomial(4);
			int segmentCount = 1;
			
			Optimizer optimizer = new Optimizer(curveSpecifications, segmentCurveTemplate, segmentCount);

			IEnumerable<Kurve.Curves.Curve> curves = optimizer.Optimize();
			
			context.LineWidth = 5;
			context.LineCap = LineCap.Round;

			foreach (Kurve.Curves.Curve curve in curves)
				DrawParametricCurve(context, curve, new Cairo.Color(0, 0, 1));

			DrawCurveSpecifications(context, curveSpecifications);

			context.Target.Dispose();
		}
	}

	static Vector2Double TransformPoint(Vector2Double point)
	{
		return 256 * point + new Vector2Double(256, 256);
	}
	static void DrawPoint(Context context, Vector2Double point, Cairo.Color color)
	{
		point = TransformPoint(point);

		context.MoveTo(point.X, point.Y);
		context.LineTo(point.X, point.Y);

		context.Color = color;
		context.Stroke();
	}
	static void DrawLine(Context context, Vector2Double startPoint, Vector2Double endPoint, Cairo.Color color)
	{
		startPoint = TransformPoint(startPoint);
		endPoint = TransformPoint(endPoint);

		context.MoveTo(startPoint.X, startPoint.Y);
		context.LineTo(endPoint.X, endPoint.Y);

		context.Color = color;
		context.Stroke();
	}
	static void DrawCurveSpecifications(Context context, IEnumerable<CurveSpecification> curveSpecifications)
	{
		foreach (CurveSpecification curveSpecification in curveSpecifications)
		{
			if (curveSpecification is PointCurveSpecification)
			{
				PointCurveSpecification pointCurveSpecification = (PointCurveSpecification)curveSpecification;

				DrawPoint(context, pointCurveSpecification.Point, new Cairo.Color(1, 0, 0));
			}
		}
	}
	static void DrawParametricCurve(Context context, Kurve.Curves.Curve curve, Cairo.Color color)
	{
		Vector2Double startPoint = TransformPoint(curve.EvaluatePoint(0));
		context.MoveTo(startPoint.X, startPoint.Y);

		for (double position = 0; position <= 1; position += 0.001)
		{
			Vector2Double point = TransformPoint(curve.EvaluatePoint(position));
			context.LineTo(point.X, point.Y);
		}
		
		context.Color = color;
		context.Stroke();
	}
}
