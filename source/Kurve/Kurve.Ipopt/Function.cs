using System;
using Krach.Basics;
using System.Collections.Generic;

namespace Kurve.Ipopt
{
	public abstract class Function
	{
		public abstract int DomainDimension { get; }
		public abstract int CodomainDimension { get; }

		public abstract IEnumerable<Matrix> GetValues(Matrix position);
        public abstract IEnumerable<Matrix> GetGradients(Matrix position);
		public abstract IEnumerable<Matrix> GetHessians(Matrix position);
	}
}

