using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Cairo;

namespace Kurve
{
	static class Drawing
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
		public static void DrawRectangle(Context context, Orthotope2Double bounds, Krach.Graphics.Color color, bool fill)
		{
			context.Rectangle(bounds.Start.X + 0.5, bounds.Start.Y + 0.5, bounds.Size.X - 1, bounds.Size.Y - 1);
			
			context.LineWidth = 1;
			context.LineCap = LineCap.Butt;
			context.Color = Drawing.ToCairoColor(color);

			if (fill) context.Fill();
			else context.Stroke();
		}
		public static void DrawSurface(Context context, Surface surface)
		{
			surface.Show(context, 0, 0);
		}
	}
}