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
	class PointSpecificationComponent : SpecificationComponent
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);
		Vector2Double point;

		Orthotope2Double Bounds { get { return new Orthotope2Double(point - 0.5 * size, point + 0.5 * size); } }

		public Vector2Double Point { get { return point; } }

		public PointSpecificationComponent(Component parent, double position, Vector2Double point) : base(parent, position)
		{
			this.point = point;
		}

		public override void Draw(Context context)
		{
			context.Rectangle(Bounds.Start.X + 0.5, Bounds.Start.Y + 0.5, Bounds.Size.X - 1, Bounds.Size.Y - 1);
			
			context.LineWidth = 1;
			context.LineCap = LineCap.Butt;
			context.Color = InterfaceUtility.ToCairoColor(Colors.Black);

			if (Selected) context.Fill();
			else context.Stroke();

			base.Draw(context);
		}

		public override void MouseMove(Vector2Double mousePosition)
		{
			if (IsMouseDown) 
			{
				point = mousePosition;

				OnSpecificationChanged();
				Changed();
			}
			
			base.MouseMove(mousePosition);
		}


		public override bool Contains (Vector2Double position)
		{
			return Bounds.Contains(position);
		}
	}
}

