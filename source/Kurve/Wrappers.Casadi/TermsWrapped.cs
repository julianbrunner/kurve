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
	static class TermsWrapped
	{
		static object synchronization = new object();

		public static ValueTerm Variable(string name, int dimension)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Variable(name, dimension);

			return new ValueTerm(result);
		}
		public static FunctionTerm Abstraction(ValueTerm variable, ValueTerm value)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Abstraction(variable.Value, value.Value);

			return new FunctionTerm(result);
		}
		public static ValueTerm Application(FunctionTerm function, ValueTerm value)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Application(function.Function, value.Value);

			return new ValueTerm(result);
		}

		public static ValueTerm Vector(IEnumerable<ValueTerm> values)
		{
			IntPtr valuePointers = values.Select(value => value.Value).Copy();
			int valueCount = values.Count();

			IntPtr result;

			lock (synchronization) result = TermsNative.Vector(valuePointers, valueCount);

			Marshal.FreeCoTaskMem(valuePointers);

			return new ValueTerm(result);
		}
		public static ValueTerm Selection(ValueTerm value, int index)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Selection(value.Value, index);

			return new ValueTerm(result);
		}

		public static ValueTerm Constant(double value)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Constant(value);

			return new ValueTerm(result);
		}

		public static ValueTerm Sum(ValueTerm value1, ValueTerm value2)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Sum(value1.Value, value2.Value);

			return new ValueTerm(result);
		}
		public static ValueTerm Product(ValueTerm value1, ValueTerm value2)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Product(value1.Value, value2.Value);

			return new ValueTerm(result);
		}
		public static ValueTerm Exponentiation(ValueTerm value1, ValueTerm value2)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Exponentiation(value1.Value, value2.Value);

			return new ValueTerm(result);
		}
		public static ValueTerm MatrixProduct(ValueTerm value1, ValueTerm value2)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.MatrixProduct(value1.Value, value2.Value);

			return new ValueTerm(result);
		}
		public static ValueTerm Transpose(ValueTerm value)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.Transpose(value.Value);

			return new ValueTerm(result);
		}
	
		public static string ValueToString(ValueTerm value)
		{
			string result;

			lock (synchronization) result = TermsNative.ValueToString(value.Value);

			return result;
		}
		public static int ValueDimension(ValueTerm value)
		{
			int result;

			lock (synchronization) result = TermsNative.ValueDimension(value.Value);

			return result;
		}
		public static IEnumerable<double> ValueEvaluate(ValueTerm value)
		{
			IntPtr values = Enumerable.Repeat(0.0, value.Dimension).Copy();

			lock (synchronization) TermsNative.ValueEvaluate(value.Value, values);

			IEnumerable<double> result = values.Read<double>(value.Dimension);

			Marshal.FreeCoTaskMem(values);

			return result;
		}
		public static ValueTerm ValueSimplify(ValueTerm value)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.ValueSimplify(value.Value);

			return new ValueTerm(result);
		}
	
		public static string FunctionToString(FunctionTerm function)
		{
			string result;

			lock (synchronization) result = TermsNative.FunctionToString(function.Function);

			return result;
		}
		public static int FunctionDomainDimension(FunctionTerm function)
		{
			int result;

			lock (synchronization) result = TermsNative.FunctionDomainDimension(function.Function);

			return result;
		}
		public static int FunctionCodomainDimension(FunctionTerm function)
		{
			int result;

			lock (synchronization) result = TermsNative.FunctionCodomainDimension(function.Function);

			return result;
		}
		public static IEnumerable<FunctionTerm> FunctionDerivatives(FunctionTerm function)
		{
			IntPtr derivatives = Enumerable.Repeat(IntPtr.Zero, function.DomainDimension).Copy();

			lock (synchronization) TermsNative.FunctionDerivatives(function.Function, derivatives);

			IEnumerable<IntPtr> result = derivatives.Read<IntPtr>(function.DomainDimension);

			Marshal.FreeCoTaskMem(derivatives);

			return result.Select(derivative => new FunctionTerm(derivative)).ToArray();
		}
		public static FunctionTerm FunctionSimplify(FunctionTerm function)
		{
			IntPtr result;

			lock (synchronization) result = TermsNative.FunctionSimplify(function.Function);

			return new FunctionTerm(result);
		}

		public static void DisposeValue(ValueTerm value)
		{
			lock (synchronization) TermsNative.DisposeValue(value.Value);
		}
		public static void DisposeFunction(FunctionTerm function)
		{
			lock (synchronization) TermsNative.DisposeFunction(function.Function);
		}
	}
}

