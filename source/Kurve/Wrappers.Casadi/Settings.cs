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
	public class Settings
	{
		public int PrintLevel { get; set; }
		public double Tolerance { get; set; }
		public int MaximumIterationCount { get; set; }

		public Settings()
		{
			PrintLevel = 5;
			Tolerance = 1e-8;
			MaximumIterationCount = 1000;
		}

		internal void Apply(IntPtr solver)
		{
			IpoptNative.SetBooleanOption(solver, "generate_hessian", true);
			IpoptNative.SetBooleanOption(solver, "print_time", false);

			IpoptNative.SetIntegerOption(solver, "print_level", PrintLevel);
			IpoptNative.SetDoubleOption(solver, "tol", Tolerance);
			IpoptNative.SetIntegerOption(solver, "max_iter", MaximumIterationCount);
		}
	}
}

