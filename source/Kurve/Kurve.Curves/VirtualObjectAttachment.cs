using System;
using System.Collections.Generic;

namespace Kurve.Curves
{
	class VirtualObjectAttachment
	{
		readonly IEnumerable<Constraint> constraints;
		
		public IEnumerable<Constraint> Constraints { get { return constraints; } }
		
		public VirtualObjectAttachment(IEnumerable<Constraint> constraints)
		{
			if (constraints == null) throw new ArgumentNullException("constraints");
			
			this.constraints = constraints;
		}
	}
}

