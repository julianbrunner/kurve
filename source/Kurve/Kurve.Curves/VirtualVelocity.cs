using System;
using Krach.Calculus.Terms;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using System.Linq;

namespace Kurve.Curves
{
	class VirtualVelocity : VirtualObject
	{
		readonly Variable x;
		readonly Variable y;
		readonly IEnumerable<Constraint> constraints;
		
		public override IEnumerable<Variable> Variables { get { return Enumerables.Create(x, y); } }
		public override IEnumerable<Constraint> Constraints { get { return constraints; } } 
		
		public VirtualVelocity(int index, IEnumerable<CurvePoint> attachmentCurvePoints)
		{
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			if (attachmentCurvePoints == null) throw new ArgumentNullException("attachmentCurvePoints");
			
			this.x = new Variable(string.Format("v_{0}_x", index));
			this.y = new Variable(string.Format("v_{0}_y", index));
			this.constraints =
			(
				from attachmentCurvePoint in attachmentCurvePoints
				from constraint in Enumerables.Create
				(
					Constraint.CreateEqualityConstraint(x, attachmentCurvePoint.Derivative.InstantiatedParametricCurve.X),
					Constraint.CreateEqualityConstraint(y, attachmentCurvePoint.Derivative.InstantiatedParametricCurve.Y)
				)
				select constraint
			)
			.ToArray();
		}
	}
}
