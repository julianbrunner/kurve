using System;
using Krach.Basics;
using Kurve.Interface;
using Krach.Extensions;

namespace Kurve.Component
{
	abstract class SpecificationComponent : PositionedControlComponent
	{
		double position = 0;
		bool isFineGrained = false;

		public event Action SpecificationChanged;

		public double CurrentPosition
		{
			get { return position; }
			set { position = value; }
		}
		public override double Position { get { return position; } }

		public SpecificationComponent(Component parent, CurveComponent curveComponent, double position) : base(parent, curveComponent) 
		{
			if (!new OrderedRange<double>(0, 1).Contains(position)) throw new ArgumentOutOfRangeException();

			this.position = position;
		}
	
		public override void Scroll(ScrollDirection scrollDirection)
		{
			if (Selected && !IsShiftDown)
			{
				double stepSize = isFineGrained ? 0.001 : 0.01;

				switch (scrollDirection)
				{
					case ScrollDirection.Up: position -= stepSize; break;
					case ScrollDirection.Down: position += stepSize; break;
					default: throw new ArgumentException();
				}

				position = position.Clamp(0, 1);

				OnSpecificationChanged();

				Changed();
			}

			base.Scroll(scrollDirection);
		}
		public override void KeyDown(Key key)
		{
			if (key == Key.Alt)
			{
				isFineGrained = true;

				Changed();
			}

			base.KeyDown(key);
		}
		public override void KeyUp(Key key)
		{
			if (key == Key.Alt)
			{
				isFineGrained = false;

				Changed();
			}

			base.KeyUp(key);
		}

		protected void OnSpecificationChanged()
		{
			if (SpecificationChanged != null) SpecificationChanged();
		}
	}
}

