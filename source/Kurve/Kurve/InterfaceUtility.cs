using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Cairo;

namespace Kurve
{
	static class InterfaceUtility
	{
		public static Cairo.Color ToCairoColor(Krach.Graphics.Color color)
		{
			return new Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha);
		}
		public static void DrawLine(Context context, Vector2Double startPoint, Vector2Double endPoint, double lineWidth, Krach.Graphics.Color color)
		{
			context.MoveTo(startPoint.X, startPoint.Y);
			context.LineTo(endPoint.X, endPoint.Y);

			context.LineWidth = lineWidth;
			context.LineCap = LineCap.Butt;
			context.Color = ToCairoColor(color);
			context.Stroke();
		}
	}
}