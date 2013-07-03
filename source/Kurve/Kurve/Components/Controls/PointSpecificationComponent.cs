using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Cairo;
using Krach.Graphics;
using Kurve.Interface;

namespace Kurve.Component
{
	class PointSpecificationComponent : SpecificationComponent
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);

		Vector2Double point = Vector2Double.Origin;

		Orthotope2Double Bounds { get { return new Orthotope2Double(point - 0.5 * size, point + 0.5 * size); } }

		public Vector2Double Point
		{
			get { return point; }
			set { point = value; }
		}

		public PointSpecificationComponent(Component parent, CurveComponent curveComponent, double position, Vector2Double point) : base(parent, curveComponent, position)
		{
			this.point = point;
		}

		public override void Draw(Context context)
		{
			Drawing.DrawRectangle(context, Bounds, Colors.Black, Selected);

			base.Draw(context);
		}
		public override void MouseMove(Vector2Double mousePosition)
		{
			if (Dragging) 
			{
				point = mousePosition;

				OnSpecificationChanged();

				Changed();
			}
			
			base.MouseMove(mousePosition);
		}

		public override bool Contains(Vector2Double position)
		{
			return Bounds.Contains(position);
		}
	}
}

