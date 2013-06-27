using System;
using Kurve.Interface;
using Cairo;
using Krach.Basics;
using Krach.Graphics;
using Kurve.Curves;

namespace Kurve.Component
{
	class InterSpecificationComponent : PositionedControlComponent
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);

		readonly SpecificationComponent leftComponent;
		readonly SpecificationComponent rightComponent;

		Orthotope2Double Bounds { get { return new Orthotope2Double(Point - 0.5 * size, Point + 0.5 * size); } }

		public override double Position { get { return (leftComponent.Position + rightComponent.Position) / 2; } }
		public Vector2Double Point { get { return Curve == null ? Vector2Double.Origin : Curve.GetPoint(Position); } }

		public InterSpecificationComponent(Component parent, CurveComponent curveComponent, SpecificationComponent leftComponent, SpecificationComponent rightComponent) : base(parent, curveComponent)
		{
			if (leftComponent == null) throw new ArgumentNullException("leftComponent");
			if (rightComponent == null) throw new ArgumentNullException("rightComponent");
		
			this.leftComponent = leftComponent;
			this.rightComponent = rightComponent;
		}

		public override void Draw(Context context)
		{
			context.Rectangle(Bounds.Start.X + 0.5, Bounds.Start.Y + 0.5, Bounds.Size.X - 1, Bounds.Size.Y - 1);
			
			context.LineWidth = 1;
			context.LineCap = LineCap.Butt;
			context.Color = InterfaceUtility.ToCairoColor(Colors.Red);

			if (Selected) context.Fill();
			else context.Stroke();

			base.Draw(context);
		}

		public override bool Contains(Vector2Double position)
		{
			return Bounds.Contains(position);
		}
	}
}
