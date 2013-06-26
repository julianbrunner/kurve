using System;
using Krach.Basics;
using Kurve.Interface;
using Krach.Extensions;

namespace Kurve.Component
{
	abstract class SpecificationComponent : PositionedControlComponent
	{
		double position = 0;

		public event Action SpecificationChanged;

		public double CurrentPosition
		{
			get { return position; }
			set { position = value; }
		}
		public override double Position { get { return position; } }

		public SpecificationComponent(Component parent, double position) : base(parent) 
		{
			if (!new OrderedRange<double>(0, 1).Contains(position)) throw new ArgumentOutOfRangeException();

			this.position = position;
		}
	
		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (Selected && !IsShiftDown)
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

		protected void OnSpecificationChanged()
		{
			if (SpecificationChanged != null) SpecificationChanged();
		}
	}
}

