using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;

namespace Kurve.Ipopt
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
			MaximumIterationCount = 3000;
		}

		internal void Apply(IntPtr problemHandle)
		{
			Wrapper.AddIpoptIntOption(problemHandle, "print_level", PrintLevel);
			Wrapper.AddIpoptNumOption(problemHandle, "tol", Tolerance);
			Wrapper.AddIpoptIntOption(problemHandle, "max_iter", MaximumIterationCount);
		}
	}
}

