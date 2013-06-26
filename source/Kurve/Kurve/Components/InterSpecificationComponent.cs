using System;
using Kurve.Interface;
using Cairo;
using Krach.Basics;
using Krach.Graphics;

namespace Kurve.Component
{
	class InterSpecificationComponent : SpecificationComponent
	{
		static readonly Vector2Double size = new Vector2Double(10, 10);

		readonly SpecificationComponent rightComponent;
		readonly SpecificationComponent leftComponent;

		DiscreteCurve discreteCurve;

		Orthotope2Double Bounds 
		{ 
			get { 
				if (discreteCurve == null) return Orthotope2Double.Empty;

				return new Orthotope2Double(discreteCurve.GetPoint(Position) - 0.5 * size, discreteCurve.GetPoint(Position) + 0.5 * size);
			} 
		}
		public DiscreteCurve DiscreteCurve { get { return discreteCurve; } set { discreteCurve = value; } }
		public override double Position { get { return GetPosition(rightComponent, leftComponent); } }

		public InterSpecificationComponent(Component parent, SpecificationComponent leftComponent, SpecificationComponent rightComponent) : base(parent, GetPosition(rightComponent, leftComponent))
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

		static double GetPosition(SpecificationComponent leftComponent, SpecificationComponent rightComponent) 
		{
			return (leftComponent.Position + rightComponent.Position) / 2;
		}
	}
}
