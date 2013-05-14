using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;
using Wrappers.Casadi.Native;
using Krach;

namespace Wrappers.Casadi
{
	public class NlpProblem : IDisposable
	{
		readonly FunctionTerm objectiveFunction;
		readonly FunctionTerm constraintFunction;
		readonly IntPtr solver;

		int DomainDimension { get { return Items.Equal(objectiveFunction.DomainDimension, constraintFunction.DomainDimension); } }
		int ObjectiveDimension { get { return objectiveFunction.CodomainDimension; } }
		int ConstraintDimension { get { return constraintFunction.CodomainDimension; } }

		bool disposed = false;

		public NlpProblem(FunctionTerm objectiveFunction, Constraint<FunctionTerm> constraint, Settings settings)
		{
			if (objectiveFunction == null) throw new ArgumentNullException("objectiveFunction");
			if (constraint == null) throw new ArgumentNullException("constraint");
			if (settings == null) throw new ArgumentNullException("settings");

			this.objectiveFunction = objectiveFunction;
			this.constraintFunction = constraint.Item;
			this.solver = IpoptNative.IpoptSolverCreate(objectiveFunction.Function, constraint.Item.Function);

			settings.Apply(solver);

			IpoptNative.IpoptSolverInitialize(solver);

			IntPtr constraintLowerBounds = constraint.Ranges.Select(range => range.Start).Copy();
			IntPtr constraintUpperBounds = constraint.Ranges.Select(range => range.End).Copy();

			IpoptNative.IpoptSolverSetConstraintBounds(solver, constraintLowerBounds, constraintUpperBounds, ConstraintDimension);

			Marshal.FreeCoTaskMem(constraintLowerBounds);
			Marshal.FreeCoTaskMem(constraintUpperBounds);
		}
		~NlpProblem()
		{
			Dispose();
		}

		public IEnumerable<double> Solve(IEnumerable<double> startPosition)
		{
			IntPtr position = startPosition.Copy();

			IpoptNative.IpoptSolverSetInitialPosition(solver, position, DomainDimension);
			IpoptNative.IpoptSolverSolve(solver);
			IpoptNative.IpoptSolverGetResultPosition(solver, position, DomainDimension);

			IEnumerable<double> resultPosition = position.Read<double>(DomainDimension);

			Marshal.FreeCoTaskMem(position);

			return resultPosition;
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				IpoptNative.IpoptSolverDispose(solver);
				
				GC.SuppressFinalize(this);
			}
		}
	}
}

