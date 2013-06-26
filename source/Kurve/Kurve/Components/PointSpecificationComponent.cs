using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Cairo;
using Krach.Graphics;
using Kurve.Interface;

namespace Kurve.Component
{
	class PointSpecificationComponent : Component
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);

		double position;
		Vector2Double point;
		bool mouseDown;
		bool dragging;
		bool selected;

		Orthotope2Double Bounds { get { return new Orthotope2Double(point - 0.5 * size, point + 0.5 * size); } }

		public event Action SpecificationChanged;

		public double Position { get { return position; } }
		public Vector2Double Point { get { return point; } }
		public bool Selected { get { return selected; } }

		public PointSpecificationComponent(Component parent, double position, Vector2Double point) : base(parent)
		{
			this.position = position;
			this.point = point;
			this.mouseDown = false;
			this.dragging = false;
			this.selected = false;
		}

		public override void Draw(Context context)
		{
			context.Rectangle(Bounds.Start.X + 0.5, Bounds.Start.Y + 0.5, Bounds.Size.X - 1, Bounds.Size.Y - 1);
			
			context.LineWidth = 1;
			context.LineCap = LineCap.Butt;
			context.Color = InterfaceUtility.ToCairoColor(Colors.Black);

			if (selected) context.Fill();
			else context.Stroke();

			base.Draw(context);
		}
		public override void MouseDown(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (Bounds.Contains(mousePosition) && mouseButton == MouseButton.Left)
			{
				mouseDown = true;

				Changed();
			}

			base.MouseDown(mousePosition, mouseButton);
		}
		public override void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
		{
			if (mouseDown && mouseButton == MouseButton.Left)
			{
				if (!dragging) selected = !selected;
				mouseDown = false;
				dragging = false;

				Changed();
			}
			
			base.MouseUp(mousePosition, mouseButton);
		}
		public override void MouseMove(Vector2Double mousePosition)
		{
			if (mouseDown) 
			{
				point = mousePosition;
				dragging = true;
				
				OnSpecificationChanged();
				Changed();
			}
			
			base.MouseMove(mousePosition);
		}
		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (selected)
			{
				switch (scrollDirection)
				{
					case ScrollDirection.Up: position -= 0.01; break;
					case ScrollDirection.Down: position += 0.01; break;
					default: throw new ArgumentException();
				}

				position = position.Clamp(0, 1);

				OnSpecificationChanged();
				Changed();
			}

			base.Scroll(scrollDirection);
		}

		void OnSpecificationChanged()
		{
			if (SpecificationChanged != null) SpecificationChanged();
		}
	}
}

