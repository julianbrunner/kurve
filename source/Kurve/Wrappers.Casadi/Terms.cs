using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Basics;

namespace Wrappers.Casadi
{
	public static class Terms
	{
		// lambda terms
		public static ValueTerm Variable(string name, int dimension)
		{
			return TermsWrapped.Variable(name, dimension);
		}
		public static ValueTerm Variable(string name)
		{
			return Variable(name, 1);
		}
		public static FunctionTerm Abstraction(ValueTerm variable, ValueTerm value)
		{
			return TermsWrapped.Abstraction(variable, value);
		}
		public static FunctionTerm Abstract(this ValueTerm value, IEnumerable<ValueTerm> variables)
		{
			return Abstraction(Vector(variables), value);
		}
		public static FunctionTerm Abstract(this ValueTerm value, params ValueTerm[] variables)
		{
			return value.Abstract((IEnumerable<ValueTerm>)variables);
		}
		public static ValueTerm Application(FunctionTerm function, ValueTerm value)
		{
			return TermsWrapped.Application(function, value);
		}
		public static ValueTerm Apply(this FunctionTerm function, IEnumerable<ValueTerm> parameters)
		{
			return Application(function, Vector(parameters));
		}
		public static ValueTerm Apply(this FunctionTerm function, params ValueTerm[] parameters)
		{
			return function.Apply((IEnumerable<ValueTerm>)parameters);
		}

		// vectors
		public static ValueTerm Vector(IEnumerable<ValueTerm> values)
		{
			return TermsWrapped.Vector(values);
		}
		public static ValueTerm Vector(params ValueTerm[] values)
		{
			return Vector((IEnumerable<ValueTerm>)values);
		}
		public static ValueTerm Selection(ValueTerm value, int index)
		{
			return TermsWrapped.Selection(value, index);
		}
		public static ValueTerm Select(this ValueTerm value, int index)
		{
			return Selection(value, index);
		}

		// constants
		public static ValueTerm Constant(double value)
		{
			return TermsWrapped.Constant(value);
		}
		public static ValueTerm Constant(IEnumerable<double> values)
		{
			return Vector(values.Select(Constant).ToArray());
		}
		public static ValueTerm Constant(params double[] values)
		{
			return Constant((IEnumerable<double>)values);
		}

		// common operations in calculus
		public static ValueTerm Sum(ValueTerm value1, ValueTerm value2)
		{
			if (value1.Dimension != value2.Dimension) throw new ArgumentException("Dimensions of 'value1' and 'value2' do not match.");

			return TermsWrapped.Sum(value1, value2);
		}
		public static ValueTerm Sum(IEnumerable<ValueTerm> values)
		{
			if (!values.Any()) throw new ArgumentException("Parameter 'values' does not contain any items.");

			return values.Aggregate(Sum);
		}
		public static ValueTerm Sum(params ValueTerm[] values)
		{
			return Sum((IEnumerable<ValueTerm>)values);
		}
		public static ValueTerm Negate(ValueTerm value)
		{
			return Scaling(Constant(-1), value);
		}
		public static ValueTerm Difference(ValueTerm value1, ValueTerm value2)
		{
			return Sum(value1, Negate(value2));
		}
		public static ValueTerm Product(ValueTerm value1, ValueTerm value2)
		{
			if (value1.Dimension != 1) throw new ArgumentException("Dimension of 'value1' is not 1.");
			if (value2.Dimension != 1) throw new ArgumentException("Dimension of 'value2' is not 1.");

			return TermsWrapped.Product(value1, value2);
		}
		public static ValueTerm Product(IEnumerable<ValueTerm> values)
		{
			if (!values.Any()) throw new ArgumentException("Parameter 'values' does not contain any items.");

			return values.Aggregate(Product);
		}
		public static ValueTerm Product(params ValueTerm[] values)
		{
			return Product((IEnumerable<ValueTerm>)values);
		}
		public static ValueTerm DotProduct(ValueTerm value1, ValueTerm value2)
		{
			if (value1.Dimension != value2.Dimension) throw new ArgumentException("Dimensions of 'value1' and 'value2' do not match.");

			return TermsWrapped.MatrixProduct(TermsWrapped.Transpose(value1), value2);
		}
		public static ValueTerm Scaling(ValueTerm factor, ValueTerm value)
		{
			if (factor.Dimension != 1) throw new ArgumentException("Dimension of 'factor' is not 1.");

			return TermsWrapped.Product(factor, value);
		}
		public static ValueTerm Invert(ValueTerm value)
		{
			return Exponentiation(value, Constant(-1));
		}
		public static ValueTerm Quotient(ValueTerm value1, ValueTerm value2)
		{
			return Product(value1, Invert(value2));
		}
		public static ValueTerm Exponentiation(ValueTerm @base, ValueTerm exponent)
		{
			if (@base.Dimension != 1) throw new ArgumentException("Dimension of 'base' is not 1.");
			if (exponent.Dimension != 1) throw new ArgumentException("Dimension of 'exponent' is not 1.");

			return TermsWrapped.Exponentiation(@base, exponent);
		}
		public static ValueTerm Square(ValueTerm value)
		{
			return Exponentiation(value, Constant(2));
		}

		public static ValueTerm Norm(ValueTerm value)
		{
			return Exponentiation(DotProduct(value, value), Constant(0.5));
		}

		public static FunctionTerm Polynomial(int dimension, int degree)
		{
			if (dimension < 0) throw new ArgumentOutOfRangeException("dimension");
			if (degree < 0) throw new ArgumentOutOfRangeException("degree");


			ValueTerm variable = Variable("x");
			IEnumerable<ValueTerm> coefficients =
			(
				from index in Enumerable.Range(0, degree)
				select Variable(string.Format("c_{0}", index), dimension)
			)
			.ToArray();

			if (degree == 0) return Constant(Enumerable.Repeat(0.0, dimension)).Abstract(Enumerables.Concatenate(Enumerables.Create(variable), coefficients));

			return Sum
			(
				from index in Enumerable.Range(0, degree)
				let power = Exponentiation(variable, Constant(index))
				let parameter = coefficients.ElementAt(index)
				select Scaling(power, parameter)
			)
			.Abstract(Enumerables.Concatenate(Enumerables.Create(variable), coefficients));
		}
		public static ValueTerm IntegrateTrapezoid(FunctionTerm function, OrderedRange<double> bounds, int segmentCount)
		{
			if (segmentCount < 1) throw new ArgumentOutOfRangeException("segmentCount");

			ValueTerm segmentWidth = Terms.Constant(bounds.Length() / segmentCount);

			IEnumerable<ValueTerm> values =
			(
				from segmentPosition in Scalars.GetIntermediateValues(bounds.Start, bounds.End, segmentCount)
				select function.Apply(Terms.Constant(segmentPosition))
			)
			.ToArray();

			return Terms.Product
			(
				segmentWidth,
				Terms.Sum
				(
					Enumerables.Concatenate
					(
						Enumerables.Create(Terms.Product(Terms.Constant(0.5), values.First())),
						values.Skip(1).SkipLast(1),
						Enumerables.Create(Terms.Product(Terms.Constant(0.5), values.Last()))
					)
				)
			);
		}
	}
}

