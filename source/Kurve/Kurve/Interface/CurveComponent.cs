//using System;
//using System.Linq;
//using System.Collections.Generic;
//using Krach.Basics;
//using Krach.Extensions;
//using Wrappers.Casadi;
//using Cairo;
//using Krach.Graphics;
//
//namespace Kurve.Interface
//{
//	class CurveComponent : Component
//	{
//		public CurveComponent()
//		{
//		}
//
//		public override void Draw(Context context)
//		{
//			double stepLength = 0.01;
//
//			for (double position = 0; position < 1; position += stepLength)
//			{
//				Krach.Graphics.Color color = Krach.Graphics.Color.InterpolateHsv(startColor, endColor, Scalars.InterpolateLinear, position);
//
//				DrawLine(context, EvaluatePoint(curve, position), EvaluatePoint(curve, position + stepLength), color);
//			}
//		}
//		public override void MouseDown(Vector2Double mousePosition, MouseButton mouseButton)
//		{
//			if (Bounds.Contains(mousePosition) && mouseButton == MouseButton.Left) dragging = true;
//		}
//		public override void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
//		{
//			if (mouseButton == MouseButton.Left) dragging = false;
//		}
//		public override void MouseMove(Vector2Double mousePosition)
//		{
//			if (dragging) 
//			{
//				position = mousePosition;
//				
//				OnUpdate();
//			}	
//		}
//		public override void Scroll(ScrollDirection scrollDirection) { }
//
//		static Vector2Double EvaluatePoint(FunctionTerm curve, double position)
//		{
//			IEnumerable<double> result = curve.Apply(Terms.Constant(position)).Evaluate();
//
//			return new Vector2Double(result.ElementAt(0), result.ElementAt(1));
//		}
//	}
//}
//
