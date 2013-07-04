using System;
using Krach.Basics;
using Kurve.Curves;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using Krach.Graphics;
using Krach.Extensions;
using Kurve.Interface;

namespace Kurve.Component
{
	class AnySpecificationComponent : SpecificationComponent
	{		
		static readonly Vector2Double size = new Vector2Double(10, 10);

		bool specifiesPoint = false;
		bool specifiesDirection = false;
		bool specifiesCurvature = false;

		Vector2Double point;
		Vector2Double direction;
		double curvature;

		public bool SpecifiesPoint { get { return specifiesPoint; } set { specifiesPoint = value; } }
		public bool SpecifiesDirection { get { return specifiesDirection; } set { specifiesDirection = value; } }
		public bool SpecifiesCurvature { get { return specifiesCurvature; } set { specifiesCurvature = value; } }

		public Vector2Double Point 
		{ 
			get 
			{ 
				if (specifiesPoint) return point;
				else if (Curve != null) return Curve.GetPoint(Position);
				else return Vector2Double.Origin;			}
			set 
			{
				if (specifiesPoint) point = value;
			}
		}
		public Vector2Double Direction 
		{ 
			get 
			{ 
				return direction; 
			}
			set 
			{
				if (specifiesDirection) direction = value;
			}
		}
		public double Curvature 
		{ 
			get 
			{ 
				return curvature; 
			}
			set 
			{
				if (specifiesCurvature) curvature = value;
			}
		}
		public IEnumerable<CurveSpecification> Specifications {
			get
			{
				List<CurveSpecification> specifications = new List<CurveSpecification>();

				if (specifiesPoint) specifications.Add(new PointCurveSpecification(Position, point));
				if (specifiesDirection) specifications.Add(new DirectionCurveSpecification(Position, direction));
				if (specifiesCurvature) specifications.Add(new CurvatureCurveSpecification(Position, curvature));

				return specifications;
			}
		}
		Orthotope2Double Bounds { 
			get 
			{ 
				return new Orthotope2Double(Point - 0.5 * size, Point + 0.5 * size); 
			} 
		}


		public AnySpecificationComponent(Component parent, CurveComponent curveComponent, double position, IEnumerable<CurveSpecification> specifications) : base(parent, curveComponent, position)
		{
			if (specifications == null) throw new ArgumentNullException("specifications");
			if (specifications.OfType<PointCurveSpecification>().Count() > 1) throw new ArgumentException("There may not be more than one PointCurveSpecification at one position.");
			if (specifications.OfType<DirectionCurveSpecification>().Count() > 1) throw new ArgumentException("There may not be more than one DirectionCurveSpecification at one position.");
			if (specifications.OfType<CurvatureCurveSpecification>().Count() > 1) throw new ArgumentException("There may not be more than one CurvatureCurveSpecification at one position.");

			if (specifications.OfType<PointCurveSpecification>().Count() == 1) {
				this.specifiesPoint = true;
				this.point = specifications.OfType<PointCurveSpecification>().Single().Point;
			}
			if (specifications.OfType<DirectionCurveSpecification>().Count() == 1) {
				this.specifiesDirection = true;
				this.direction = specifications.OfType<DirectionCurveSpecification>().Single().Direction;
			}
			if (specifications.OfType<CurvatureCurveSpecification>().Count() == 1) {
				this.specifiesCurvature = true;
				this.curvature = specifications.OfType<CurvatureCurveSpecification>().Single().Curvature;
			}
		}

		public override void Draw(Context context)
		{
			Drawing.DrawRectangle(context, Bounds, Colors.Black, Selected);

			string text = (specifiesPoint ? "P" : "") + (specifiesDirection ? "D" : "") + (specifiesCurvature ? "C" : "");
			if (!(specifiesPoint || specifiesDirection || SpecifiesCurvature)) text = "n/a";
			Drawing.DrawText(context, text, Bounds.Start + new Vector2Double(-10, Bounds.Size.Y + 15), Colors.Black);
			base.Draw(context);
		}

		public override void MouseMove(Vector2Double mousePosition)
		{
			if (Dragging) 
			{
				if (IsShiftDown) 
				{
					double closestPosition = 
					(
						from position in Scalars.GetIntermediateValuesSymmetric(0, 1, 100)
						let distance = (Curve.GetPoint(position) - mousePosition).Length
						orderby distance ascending
						select position
					)
					.First();

					CurrentPosition = closestPosition;
					point = Curve.GetPoint(closestPosition);
				}
				else 
				{
					Point += DragVector * SlowDownFactor;
				}

				OnSpecificationChanged();
				Changed();
			}
			
			base.MouseMove(mousePosition);
		}

		public override void KeyDown(Key key)
		{
			base.KeyDown(key);
		}
		public override void KeyUp(Key key)
		{
			if (Selected) 
			{
				bool needsSpecificationChangedEvent = false;

				switch (key) 
				{
					case Key.P: 
						specifiesPoint = !specifiesPoint; 
						point = Curve.GetPoint(Position);
						needsSpecificationChangedEvent = true;
						break;
					case Key.D:
						specifiesDirection = !specifiesDirection;
						direction = Curve.GetDirection(Position);
						needsSpecificationChangedEvent = true;
						break;
					case Key.C: 
						specifiesCurvature = !specifiesCurvature;
						curvature = Curve.GetCurvature(Position);
						needsSpecificationChangedEvent = true;
						break;
					default: break;
				}

				if (needsSpecificationChangedEvent) OnSpecificationChanged();
			}
			base.KeyUp(key);
		}

		public override bool Contains(Vector2Double position)
		{
			return Bounds.Contains(position);
		}
	}
}

