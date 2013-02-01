using System;
using Krach.Calculus.Terms;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using System.Linq;

namespace Kurve.Curves
{
	class VirtualDirection : VirtualObject
	{
		readonly Variable x;
		readonly Variable y;
		readonly IEnumerable<VirtualObjectAttachment> attachments;
		readonly Term errorTerm;
		
		public override IEnumerable<Variable> Variables { get { return Enumerables.Create(x, y); } }
		public override IEnumerable<Constraint> Constraints 
		{
			get 
			{
				return 
					from attachment in attachments
					from constraint in attachment.Constraints
					select constraint;
			}
		} 
		public override Term ErrorTerm { get { return errorTerm; } }
		
		public VirtualDirection(int index, IEnumerable<VirtualObjectAttachmentSpecification> specifications, Vector2Double direction)
		{
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			if (specifications == null) throw new ArgumentNullException("specification");
			
			//direction = direction.NormalizedVector;
			
			this.x = new Variable(string.Format("v_{0}_x", index));
			this.y = new Variable(string.Format("v_{0}_y", index));
			
			this.attachments = specifications.Select(CreateAttachment);	
			this.errorTerm = Term.Sum
			(
				Term.Difference(x, Term.Constant(direction.X)).Square(),
				Term.Difference(y, Term.Constant(direction.Y)).Square()
			);
		}
		
		VirtualObjectAttachment CreateAttachment(VirtualObjectAttachmentSpecification specification)
		{
			//
			// 1. Graphic concept of direction => mathematical concept
			// 		Velocity == 0 (or direction undefined) does not mean no direction in graphical sense
			// 		Limit of direction better?
			// 2. Perhaps not model as constraint function, think about direction equality first
			// 3. Perhaps exclude curves with 0-velocity because of infeasibility?
			//
			
			ParametricCurve instantiatedCurve = specification.ParametricCurve.Derivative.InstantiatePosition(Term.Constant(specification.Position));
//			Term velocityMagnitude = Term.Sum
//			(
//				instantiatedCurve.X.Square(), 
//				instantiatedCurve.Y.Square()
//			)
//			.Exponentiate(new Constant(0.5));
			
			return new VirtualObjectAttachment
			(
				Enumerables.Create
				(
					new Constraint(Term.Difference(x, instantiatedCurve.X), new OrderedRange<double>(0)),
					new Constraint(Term.Difference(y, instantiatedCurve.Y), new OrderedRange<double>(0))
				)
			);
		}
	}
}

