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
//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double(-0.5,  0.0)),
//				new VelocityCurveSpecification(0.0, new Vector2Double( 0.0, +1.0)),
//				new PointCurveSpecification(0.5, new Vector2Double(-0.25, +0.5)),
//				new VelocityCurveSpecification(0.5, new Vector2Double(+5.0,  0.0)),
//				new PointCurveSpecification(1.0, new Vector2Double(+0.5, -0.25)),
//				new VelocityCurveSpecification(1.0, new Vector2Double( 0.0, -10.0))
//			);
//			CurveTemplate segmentCurveTemplate = CurveTemplate.CreatePolynomial(4);
//			int segmentCount = 2;

//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double(0, 0)),
//				new VelocityCurveSpecification(0.2, new Vector2Double(0.5, 0.5)),
//				new VelocityCurveSpecification(0.5, new Vector2Double(0.5, 0.0)),
//				new VelocityCurveSpecification(0.9, new Vector2Double(0.5, 0.5)),
//				new PointCurveSpecification(1.0, new Vector2Double(0.5, 0.5))
//			);
//			CurveTemplate segmentCurveTemplate = CurveTemplate.CreatePolynomial(4);
//			int segmentCount = 1;

//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double(-0.5, -0.5)),
//				new PointCurveSpecification(0.2, new Vector2Double(+0.5, +0.5)),
//				new PointCurveSpecification(0.4, new Vector2Double(+0.5, -0.5)),
//				new PointCurveSpecification(0.6, new Vector2Double(-0.5, +0.5)),
//				new PointCurveSpecification(0.8, new Vector2Double( 0.0,  0.0)),
//				new PointCurveSpecification(1.0, new Vector2Double( 0.0, -0.5))
//			);
//			CurveTemplate segmentCurveTemplate = CurveTemplate.CreatePolynomial(3);
//			int segmentCount = 4;

//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double(-0.5, -0.5)),
//				new PointCurveSpecification(0.3, new Vector2Double( 0.0, +0.5)),
//				new PointCurveSpecification(1.0, new Vector2Double(+0.5, -0.5))
//			);
//			CurveTemplate segmentCurveTemplate = CurveTemplate.CreatePolynomial(5);
//			int segmentCount = 2;

//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double(-0.5, -0.5)),
//				new PointCurveSpecification(0.3, new Vector2Double( 0.0, +0.5)),
//				new VelocityCurveSpecification(0.7, new Vector2Double(+4.0,  0.0)),
//				new PointCurveSpecification(1.0, new Vector2Double(+0.5, -0.5))
//			);
//			CurveTemplate segmentCurveTemplate = CurveTemplate.CreatePolynomial(5);
//			int segmentCount = 4;

//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double(-1.0, -1.0)),
//				new PointCurveSpecification(0.3, new Vector2Double(-0.5,  0.0)),
//				new PointCurveSpecification(0.5, new Vector2Double( 0.0,  0.0)),
//				new PointCurveSpecification(0.7, new Vector2Double(+0.5,  0.0)),
//				new PointCurveSpecification(1.0, new Vector2Double(+1.0, +1.0))
//			);
//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double(-1.0,  0.0)),
//				new VelocityCurveSpecification(0.0, new Vector2Double( 0.0, -4.0)),
//
//				new PointCurveSpecification(1.0, new Vector2Double(+1.0,  0.0)),
//				new VelocityCurveSpecification(1.0, new Vector2Double( 0.0, +4.0))
//			);
//			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
//			(
//				new PointCurveSpecification(0.0, new Vector2Double( 0.0,  0.0)),
//				new VelocityCurveSpecification(0.0, new Vector2Double( 0.0, +4.0)),
//				new PointCurveSpecification(1.0, new Vector2Double( 0.0,  0.0)),
//				new VelocityCurveSpecification(1.0, new Vector2Double( 0.0, +4.0))
//			);
			IEnumerable<PositionedCurveSpecification> curveSpecifications = Enumerables.Create<PositionedCurveSpecification>
			(
				new PointCurveSpecification(0.0, new Vector2Double(-1.0,  0.0)),
				new AccelerationCurveSpecification(0.5, new Vector2Double( 0.0, +16.0)),
				new PointCurveSpecification(1.0, new Vector2Double(+1.0,  0.0))
			);
			CurveTemplate segmentCurveTemplate = CurveTemplate.CreatePolynomial(4);
			int segmentCount = 10;
			double curveLength = 4;
			
			Optimizer optimizer = new Optimizer(curveSpecifications, segmentCurveTemplate, segmentCount, curveLength);

			IEnumerable<Kurve.Curves.Curve> result = optimizer.Optimize();

			double segmentLength = curveLength / segmentCount;
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

			DrawCurveSpecifications(context, curveSpecifications);

			context.Target.Dispose();
		}
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
	static void DrawCurveSpecifications(Context context, IEnumerable<PositionedCurveSpecification> curveSpecifications)
	{
		foreach (PositionedCurveSpecification curveSpecification in curveSpecifications)
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