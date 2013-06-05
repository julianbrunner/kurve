using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;
using Wrappers.Casadi.Native;

namespace Wrappers.Casadi
{
	public class IpoptProblem : IDisposable
	{
		readonly IntPtr problem;
		readonly int domainDimension;

		bool disposed = false;

		public IntPtr Problem { get { return problem; } }
		public int DomainDimension { get { return domainDimension; } }

		IpoptProblem(IntPtr problem, int domainDimension)
		{
			if (problem == IntPtr.Zero) throw new ArgumentOutOfRangeException("problem");
			if (domainDimension < 0) throw new ArgumentOutOfRangeException("domainDimension");

			this.problem = problem;
			this.domainDimension = domainDimension;
		}
		~IpoptProblem()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				lock (GeneralNative.Synchronization) IpoptNative.IpoptProblemDispose(problem);
				
				GC.SuppressFinalize(this);
			}
		}
		public IpoptProblem Substitute(ValueTerm variable, ValueTerm value)
		{
			IntPtr newProblem;
			lock (GeneralNative.Synchronization) newProblem = IpoptNative.IpoptProblemSubstitute(problem, variable.Value, value.Value);

			return new IpoptProblem(newProblem, domainDimension);
		}

		public static IpoptProblem Create(FunctionTerm objectiveFunction, FunctionTerm constraintFunction)
		{
			IntPtr problem;
			lock (GeneralNative.Synchronization) problem = IpoptNative.IpoptProblemCreate(objectiveFunction.Function, constraintFunction.Function);

			int domainDimension = Items.Equal(objectiveFunction.DomainDimension, constraintFunction.DomainDimension);

			return new IpoptProblem(problem, domainDimension);
		}
	}
}

