using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;

namespace Wrappers.Casadi
{
	public class ValueTerm : IDisposable
	{
		readonly IntPtr value;

		bool disposed = false;

		public IntPtr Value { get { return value; } }
		public int Dimension { get { return TermsWrapped.ValueDimension(this); } }

		public ValueTerm(IntPtr value)
		{
			if (value == IntPtr.Zero) throw new ArgumentOutOfRangeException("value");

			this.value = value;
		}
		~ValueTerm()
		{
			Dispose();
		}

		public override string ToString()
		{
			return TermsWrapped.ValueToString(this);
		}
		public IEnumerable<double> Evaluate()
		{
			return TermsWrapped.ValueEvaluate(this);
		}
		public ValueTerm Simplify()
		{
			return TermsWrapped.ValueSimplify(this);
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				TermsWrapped.DisposeValue(this);
				
				GC.SuppressFinalize(this);
			}
		}
	}
}

