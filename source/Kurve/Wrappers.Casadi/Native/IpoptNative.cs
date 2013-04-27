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
		public static extern IntPtr IpoptSolverCreate(IntPtr objectiveFunction, IntPtr constraintFunction);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverInitialize(IntPtr solver);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverSetConstraintBounds(IntPtr solver, IntPtr constraintLowerBounds, IntPtr constraintUpperBounds, int constraintCount);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverSetInitialPosition(IntPtr solver, IntPtr position, int positionCount);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverSolve(IntPtr solver);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverGetResultPosition(IntPtr solver, IntPtr position, int positionCount);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void IpoptSolverDispose(IntPtr solver);
		
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void SetBooleanOption(IntPtr function, string name, bool value);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void SetIntegerOption(IntPtr function, string name, int value);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void SetDoubleOption(IntPtr function, string name, double value);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void SetStringOption(IntPtr function, string name, string value);
	}
}

