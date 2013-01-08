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
			ParametricCurve parametricCurve = ParametricCurve.CreatePolynomialParametricCurveTemplate(3).InstantiateParameters
			(
				from coefficientValue in Enumerables.Create<double>(10, 10, 10, 200, 60, 10)
				select Term.Constant(coefficientValue)
			);

			DrawParametricCurve(context, parametricCurve);

			context.Target.Dispose();
		}
	}

	static void DrawParametricCurve(Context context, ParametricCurve curve)
	{
		Vector2Double startPoint = curve.EvaluatePoint(0);
		context.MoveTo(startPoint.X, startPoint.Y);

		for (double position = 0; position <= 1; position += 0.01)
		{
			Vector2Double point = curve.EvaluatePoint(position);
			context.LineTo(point.X, point.Y);
		}
		
		context.Color = new Cairo.Color(0, 0, 1);
		context.Stroke();
	}
}
