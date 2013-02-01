using System;
using Krach.Calculus.Terms;
using Krach.Extensions;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;

namespace Kurve.Curves
{
	class VirtualPoint : VirtualObject
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
		
		public VirtualPoint(int index, IEnumerable<VirtualObjectAttachmentSpecification> specifications, Vector2Double point)
		{
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			if (specifications == null) throw new ArgumentNullException("specification");
			
			this.x = new Variable(string.Format("p_{0}_x", index));
			this.y = new Variable(string.Format("p_{0}_y", index));
			
			this.attachments = specifications.Select(CreateAttachment);	
			this.errorTerm = Term.Sum
			(
				Term.Difference(x, Term.Constant(point.X)).Square(),
				Term.Difference(y, Term.Constant(point.Y)).Square()
			);
		}
		
		VirtualObjectAttachment CreateAttachment(VirtualObjectAttachmentSpecification specification)
		{
			ParametricCurve instantiatedCurve = specification.ParametricCurve.InstantiatePosition(Term.Constant(specification.Position));
			
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

