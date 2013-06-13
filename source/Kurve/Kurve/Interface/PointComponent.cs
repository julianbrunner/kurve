using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Cairo;
using Krach.Graphics;

namespace Kurve.Interface
{
	class PointComponent : Component
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);

		double position;
		Vector2Double point;
		bool dragging;

		Orthotope2Double Bounds { get { return new Orthotope2Double(point - 0.5 * size, point + 0.5 * size); } }

		public double Position { get { return position; } }
		public Vector2Double Point { get { return point; } }

		public PointComponent(double position, Vector2Double point)
		{
			this.position = position;
			this.point = point;
			this.dragging = false;
			
			OnUpdate();
		}

		public override void Draw(Context context)
		{
			context.Rectangle(Bounds.Start.X + 0.5, Bounds.Start.Y + 0.5, Bounds.Size.X - 1, Bounds.Size.Y - 1);
			
			context.LineWidth = 1;
			context.LineCap = LineCap.Butt;
			context.Color = InterfaceUtility.ToCairoColor(Colors.Black);
			context.Stroke();
		}
		public override void MouseDown(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (Bounds.Contains(mousePosition) && mouseButton == MouseButton.Left) dragging = true;
		}
		public override void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (mouseButton == MouseButton.Left) dragging = false;
		}
		public override void MouseMove(Vector2Double mousePosition)
		{
			if (dragging) 
			{
				point = mousePosition;
				
				OnUpdate();
			}	
		}
		public override void Scroll(ScrollDirection scrollDirection)
		{
			switch (scrollDirection)
			{
				case ScrollDirection.Up: position -= 0.01; break;
				case ScrollDirection.Down: position += 0.01; break;
				default: throw new ArgumentException();
			}

			OnUpdate();
		}
	}
}

