using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;

namespace Wrappers.Casadi.Native
{
	static class IpoptNative
	{
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr IpoptProblemCreate(IntPtr objectiveFunction, IntPtr constraintFunction);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptProblemDispose(IntPtr problem);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr IpoptProblemSubstitute(IntPtr problem, IntPtr variables, IntPtr values, int count);

		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr IpoptSolverCreate(IntPtr problem);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverDispose(IntPtr solver);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverInitialize(IntPtr solver);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverSetConstraintBounds(IntPtr solver, IntPtr constraintLowerBounds, IntPtr constraintUpperBounds, int constraintCount);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverSetInitialPosition(IntPtr solver, IntPtr position, int positionCount);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern string IpoptSolverSolve(IntPtr solver);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverGetResultPosition(IntPtr solver, IntPtr position, int positionCount);
	}
}

