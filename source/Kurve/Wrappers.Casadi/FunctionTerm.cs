using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;

namespace Wrappers.Casadi
{
	public class FunctionTerm : IDisposable
	{
		readonly IntPtr function;

		bool disposed = false;

		public IntPtr Function { get { return function; } }
		public int DomainDimension { get { return TermsWrapped.FunctionDomainDimension(this); } }
		public int CodomainDimension { get { return TermsWrapped.FunctionCodomainDimension(this); } }

		public FunctionTerm(IntPtr function)
		{
			if (function == IntPtr.Zero) throw new ArgumentOutOfRangeException("function");

			this.function = function;
		}
		~FunctionTerm()
		{
			Dispose();
		}
		
		public override string ToString()
		{
			return TermsWrapped.FunctionToString(this);
		}
		public IEnumerable<FunctionTerm> GetDerivatives()
		{
			return TermsWrapped.FunctionDerivatives(this);
		}
		public FunctionTerm Simplify()
		{
			return TermsWrapped.FunctionSimplify(this);
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				TermsWrapped.DisposeFunction(this);
				
				GC.SuppressFinalize(this);
			}
		}
	}
}

