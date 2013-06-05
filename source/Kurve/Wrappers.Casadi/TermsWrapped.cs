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
		public static ValueTerm Variable(string name, int dimension)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Variable(name, dimension));
		}
		public static FunctionTerm Abstraction(ValueTerm variable, ValueTerm value)
		{
			lock (GeneralNative.Synchronization) return new FunctionTerm(TermsNative.Abstraction(variable.Value, value.Value));
		}
		public static ValueTerm Application(FunctionTerm function, ValueTerm value)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Application(function.Function, value.Value));
		}

		public static ValueTerm Vector(IEnumerable<ValueTerm> values)
		{
			lock (GeneralNative.Synchronization)
			{
				IntPtr valuePointers = values.Select(value => value.Value).Copy();
				int valueCount = values.Count();

				IntPtr result = TermsNative.Vector(valuePointers, valueCount);

				Marshal.FreeCoTaskMem(valuePointers);
				
				return new ValueTerm(result);
			}
		}
		public static ValueTerm Selection(ValueTerm value, int index)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Selection(value.Value, index));
		}

		public static ValueTerm Constant(double value)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Constant(value));
		}

		public static ValueTerm Sum(ValueTerm value1, ValueTerm value2)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Sum(value1.Value, value2.Value));
		}
		public static ValueTerm Product(ValueTerm value1, ValueTerm value2)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Product(value1.Value, value2.Value));
		}
		public static ValueTerm Exponentiation(ValueTerm value1, ValueTerm value2)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Exponentiation(value1.Value, value2.Value));
		}
		public static ValueTerm MatrixProduct(ValueTerm value1, ValueTerm value2)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.MatrixProduct(value1.Value, value2.Value));
		}
		public static ValueTerm Transpose(ValueTerm value)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.Transpose(value.Value));
		}
	
		public static string ValueToString(ValueTerm value)
		{
			lock (GeneralNative.Synchronization) return TermsNative.ValueToString(value.Value);
		}
		public static int ValueDimension(ValueTerm value)
		{
			lock (GeneralNative.Synchronization) return TermsNative.ValueDimension(value.Value);
		}
		public static IEnumerable<double> ValueEvaluate(ValueTerm value)
		{
			lock (GeneralNative.Synchronization)
			{
				IntPtr values = Enumerable.Repeat(0.0, value.Dimension).Copy();

				TermsNative.ValueEvaluate(value.Value, values);

				IEnumerable<double> result = values.Read<double>(value.Dimension);

				Marshal.FreeCoTaskMem(values);

				return result;
			}
		}
		public static ValueTerm ValueSimplify(ValueTerm value)
		{
			lock (GeneralNative.Synchronization) return new ValueTerm(TermsNative.ValueSimplify(value.Value));
		}
	
		public static string FunctionToString(FunctionTerm function)
		{
			lock (GeneralNative.Synchronization) return TermsNative.FunctionToString(function.Function);
		}
		public static int FunctionDomainDimension(FunctionTerm function)
		{
			lock (GeneralNative.Synchronization) return TermsNative.FunctionDomainDimension(function.Function);
		}
		public static int FunctionCodomainDimension(FunctionTerm function)
		{
			lock (GeneralNative.Synchronization) return TermsNative.FunctionCodomainDimension(function.Function);
		}
		public static IEnumerable<FunctionTerm> FunctionDerivatives(FunctionTerm function)
		{
			lock (GeneralNative.Synchronization)
			{
				IntPtr derivatives = Enumerable.Repeat(IntPtr.Zero, function.DomainDimension).Copy();

				TermsNative.FunctionDerivatives(function.Function, derivatives);

				IEnumerable<IntPtr> result = derivatives.Read<IntPtr>(function.DomainDimension);

				Marshal.FreeCoTaskMem(derivatives);

				return result.Select(derivative => new FunctionTerm(derivative)).ToArray();
			}
		}
		public static FunctionTerm FunctionSimplify(FunctionTerm function)
		{
			lock (GeneralNative.Synchronization) return new FunctionTerm(TermsNative.FunctionSimplify(function.Function));
		}

		public static void DisposeValue(ValueTerm value)
		{
			lock (GeneralNative.Synchronization) TermsNative.DisposeValue(value.Value);
		}
		public static void DisposeFunction(FunctionTerm function)
		{
			lock (GeneralNative.Synchronization) TermsNative.DisposeFunction(function.Function);
		}
	}
}

