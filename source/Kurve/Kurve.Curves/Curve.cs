using Krach.Basics;
using Krach.Extensions;

namespace Kurve.Curves
{
	public abstract class Curve
	{
		public abstract Vector2Double GetPoint(double position);
		public abstract double GetSpeed(double position);
		public abstract double GetDirection(double position);
		public abstract double GetCurvature(double position);

		public Vector2Double GetDirectionVector(double position)
		{
			double direction = GetDirection(position);

			return new Vector2Double(Scalars.Cosine(direction), Scalars.Sine(direction));
		}
		public Vector2Double GetNormalVector(double position)
		{
			Vector2Double directionVector = GetDirectionVector(position);

			return new Vector2Double(directionVector.Y, -directionVector.X);
		}
		public Vector2Double GetVelocity(double position)
		{
			return GetSpeed(position) * GetDirectionVector(position);
		}
	}
}

