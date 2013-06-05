using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;

namespace Wrappers.Casadi.Native
{
	static class GeneralNative
	{
		public static readonly object Synchronization = new object();

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

