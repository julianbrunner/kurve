using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;

namespace Wrappers.Casadi.Native
{
	static class TermsNative
	{
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Variable(string name, int dimension);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Abstraction(IntPtr variable, IntPtr value);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Application(IntPtr function, IntPtr value);

		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Vector(IntPtr values, int valueCount);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Selection(IntPtr value, int index);

		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Constant(double value);

		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Sum(IntPtr value1, IntPtr value2);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Product(IntPtr value1, IntPtr value2);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Exponentiation(IntPtr value1, IntPtr value2);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr MatrixProduct(IntPtr value1, IntPtr value2);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr Transpose(IntPtr value);
		
		[DllImport("Wrappers.Casadi.Native")]
		public static extern string ValueToString(IntPtr value);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern int ValueDimension(IntPtr value);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void ValueEvaluate(IntPtr value, IntPtr values);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr ValueSimplify(IntPtr value);
		
		[DllImport("Wrappers.Casadi.Native")]
		public static extern string FunctionToString(IntPtr function);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern int FunctionDomainDimension(IntPtr function);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern int FunctionCodomainDimension(IntPtr function);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void FunctionDerivatives(IntPtr function, IntPtr derivatives);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern IntPtr FunctionSimplify(IntPtr function);

		[DllImport("Wrappers.Casadi.Native")]
		public static extern void DisposeValue(IntPtr value);
		[DllImport("Wrappers.Casadi.Native")]
		public static extern void DisposeFunction(IntPtr function);
	}
}

