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
		readonly IEnumerable<OrderedRange<double>> constraintRanges;
		readonly int domainDimension;

		bool disposed = false;

		public IntPtr Problem { get { return problem; } }
		public IEnumerable<OrderedRange<double>> ConstraintRanges { get { return constraintRanges; } }
		public int DomainDimension { get { return domainDimension; } }

		IpoptProblem(IntPtr problem, IEnumerable<OrderedRange<double>> constraintRanges, int domainDimension)
		{
			if (problem == IntPtr.Zero) throw new ArgumentOutOfRangeException("problem");
			if (constraintRanges == null) throw new ArgumentNullException("constraintRanges");
			if (domainDimension < 0) throw new ArgumentOutOfRangeException("domainDimension");

			this.problem = problem;
			this.constraintRanges = constraintRanges;
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
		public IpoptProblem Substitute(IEnumerable<Substitution> substitutions)
		{
			IntPtr variablesPointer = substitutions.Select(substitution => substitution.Variable.Value).Copy();
			IntPtr valuesPointer = substitutions.Select(substitution => substitution.Value.Value).Copy();

			IntPtr newProblem;
			lock (GeneralNative.Synchronization) newProblem = IpoptNative.IpoptProblemSubstitute(problem, variablesPointer, valuesPointer, substitutions.Count());

			Marshal.FreeCoTaskMem(variablesPointer);
			Marshal.FreeCoTaskMem(valuesPointer);

			return new IpoptProblem(newProblem, constraintRanges, domainDimension);
		}

		public static IpoptProblem Create(FunctionTerm objectiveFunction, FunctionTerm constraintFunction, IEnumerable<OrderedRange<double>> constraintRanges)
		{
			IntPtr problem;
			lock (GeneralNative.Synchronization) problem = IpoptNative.IpoptProblemCreate(objectiveFunction.Function, constraintFunction.Function);

			int domainDimension = Items.Equal(objectiveFunction.DomainDimension, constraintFunction.DomainDimension);

			return new IpoptProblem(problem, constraintRanges, domainDimension);
		}
	}
}

