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
		readonly IntPtr solver;
		readonly int domainDimension;

		bool disposed = false;

		public NlpProblem(IpoptProblem problem, IEnumerable<OrderedRange<double>> constraints, Settings settings)
		{
			if (problem == null) throw new ArgumentNullException("problem");
			if (constraints == null) throw new ArgumentNullException("constraints");
			if (settings == null) throw new ArgumentNullException("settings");

			this.solver = IpoptNative.IpoptSolverCreate(problem.Problem);
			this.domainDimension = problem.DomainDimension;

			settings.Apply(solver);

			IpoptNative.IpoptSolverInitialize(solver);

			IntPtr constraintLowerBounds = constraints.Select(range => range.Start).Copy();
			IntPtr constraintUpperBounds = constraints.Select(range => range.End).Copy();

			IpoptNative.IpoptSolverSetConstraintBounds(solver, constraintLowerBounds, constraintUpperBounds, constraints.Count());

			Marshal.FreeCoTaskMem(constraintLowerBounds);
			Marshal.FreeCoTaskMem(constraintUpperBounds);
		}

		public NlpProblem(FunctionTerm objectiveFunction, FunctionTerm constraintFunction, IEnumerable<OrderedRange<double>> constraints, Settings settings)
		{
			if (objectiveFunction == null) throw new ArgumentNullException("objectiveFunction");
			if (constraintFunction == null) throw new ArgumentNullException("constraintFunction");
			if (constraints == null) throw new ArgumentNullException("constraints");
			if (settings == null) throw new ArgumentNullException("settings");

			this.solver = IpoptNative.IpoptSolverCreateSimple(objectiveFunction.Function, constraintFunction.Function);
			this.domainDimension = Items.Equal(objectiveFunction.DomainDimension, constraintFunction.DomainDimension);

			settings.Apply(solver);

			IpoptNative.IpoptSolverInitialize(solver);

			IntPtr constraintLowerBounds = constraints.Select(range => range.Start).Copy();
			IntPtr constraintUpperBounds = constraints.Select(range => range.End).Copy();

			IpoptNative.IpoptSolverSetConstraintBounds(solver, constraintLowerBounds, constraintUpperBounds, constraints.Count());

			Marshal.FreeCoTaskMem(constraintLowerBounds);
			Marshal.FreeCoTaskMem(constraintUpperBounds);
		}
		~NlpProblem()
		{
			Dispose();
		}

		public IEnumerable<double> Solve(IEnumerable<double> startPosition)
		{
			if (startPosition.Count() != domainDimension) throw new ArgumentException("Parameter 'startPosition' has the wrong number of items.");

			IntPtr position = startPosition.Copy();

			IpoptNative.IpoptSolverSetInitialPosition(solver, position, domainDimension);
			IpoptNative.IpoptSolverSolve(solver);
			IpoptNative.IpoptSolverGetResultPosition(solver, position, domainDimension);

			IEnumerable<double> resultPosition = position.Read<double>(domainDimension);

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

