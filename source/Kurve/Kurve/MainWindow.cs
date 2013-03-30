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
//			CurvePlaceSpecification point1 = CurvePlaceSpecification.CreatePointVelocitySpecification(0.0, new Vector2Double(100, 100), new Vector2Double(0, 100));
//			CurvePlaceSpecification point2 = CurvePlaceSpecification.CreatePointVelocitySpecification(0.5, new Vector2Double(150, 200), new Vector2Double(500, 0));
//			CurvePlaceSpecification point3 = CurvePlaceSpecification.CreatePointVelocitySpecification(1.0, new Vector2Double(300, 50), new Vector2Double(0, -1000));
//			ParametricCurve curveTemplate = ParametricCurve.CreatePolynomialParametricCurveTemplate(3);
//			
//			Optimizer optimizer = new Optimizer(Enumerables.Create(point1, point2, point3), curveTemplate, 2);
//			
			PointCurveSpecification point1 = new PointCurveSpecification(0.2, new Vector2Double(100, 100));
			PointCurveSpecification point2 = new PointCurveSpecification(0.3, new Vector2Double(150, 200));
			PointCurveSpecification point3 = new PointCurveSpecification(0.9, new Vector2Double(300,  50));
			Kurve.Curves.Curve segmentCurve = Kurve.Curves.Curve.CreatePolynomialCurve(3);
			
			Optimizer optimizer = new Optimizer(Enumerables.Create(point1), segmentCurve, 1);
			
			foreach (Kurve.Curves.Curve curve in optimizer.Optimize()) DrawParametricCurve(context, curve);
			
			context.Target.Dispose();
		}
	}

	static void DrawParametricCurve(Context context, Kurve.Curves.Curve curve)
	{
		Vector2Double startPoint = curve.EvaluatePoint(0);
		context.MoveTo(startPoint.X, startPoint.Y);

		for (double position = 0; position <= 1; position += 0.001)
		{
			Vector2Double point = curve.EvaluatePoint(position);
			context.LineTo(point.X, point.Y);
		}
		
		context.Color = new Cairo.Color(0, 0, 1);
		context.Stroke();
	}
}
