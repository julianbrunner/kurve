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

		Orthotope2Double Bounds 
		{ 
			get
			{ 
				if (Curve == null) return Orthotope2Double.Empty;

				return new Orthotope2Double(Curve.GetPoint(Position) - 0.5 * size, Curve.GetPoint(Position) + 0.5 * size);
			} 
		}

		public override double Position { get { return (leftComponent.Position + rightComponent.Position) / 2; } }

		public InterSpecificationComponent(Component parent, SpecificationComponent leftComponent, SpecificationComponent rightComponent) : base(parent)
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
