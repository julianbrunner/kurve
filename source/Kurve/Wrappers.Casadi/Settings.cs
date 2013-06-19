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
			PrintLevel = 0;
			Tolerance = 1e-8;
			MaximumIterationCount = 10000;
		}

		internal void Apply(IntPtr solver)
		{
			GeneralNative.SetBooleanOption(solver, "generate_hessian", true);
			GeneralNative.SetBooleanOption(solver, "print_time", false);

			GeneralNative.SetIntegerOption(solver, "print_level", PrintLevel);
			GeneralNative.SetDoubleOption(solver, "tol", Tolerance);
			GeneralNative.SetIntegerOption(solver, "max_iter", MaximumIterationCount);
		}
	}
}

