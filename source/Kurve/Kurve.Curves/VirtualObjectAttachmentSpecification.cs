using System;

namespace Kurve.Curves
{
	class VirtualObjectAttachmentSpecification
	{
		readonly ParametricCurve parametricCurve;
		readonly double position;
		
		public ParametricCurve ParametricCurve { get { return parametricCurve; } }
		public double Position { get { return position; } }
		
		public VirtualObjectAttachmentSpecification(ParametricCurve parametricCurve, double position)
		{
			if (parametricCurve == null) throw new ArgumentNullException("parametricCurve");
			
			this.parametricCurve = parametricCurve;
			this.position = position;
		}
	}
}
