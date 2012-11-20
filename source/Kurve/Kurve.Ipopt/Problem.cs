using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;

namespace Kurve.Ipopt
{
	public class Problem : IDisposable
	{
		static Dictionary<IntPtr, Problem> instances = new Dictionary<IntPtr, Problem>();

		readonly Function objective;
		readonly Function constraints;
		readonly int positionSize;
		readonly int constraintsSize;
		readonly IntPtr problemHandle;
		
		bool disposed = false;

		public Problem(Function objective, Function constraints, Range<Matrix> positionRange, Range<Matrix> constraintsRange)
		{
			if (objective == null) throw new ArgumentNullException("objective");
			if (constraints == null) throw new ArgumentNullException("constraints");

			this.objective = objective;
			this.constraints = constraints;

			Vector2Integer positionSize = new Vector2Integer(Items.Equal(positionRange.Start.RowCount, positionRange.End.RowCount), Items.Equal(positionRange.Start.ColumnCount, positionRange.End.ColumnCount));
			if (positionSize.Y != 1) throw new ArgumentException("Parameter positionRange is not a row vector range.");
			this.positionSize = positionSize.X;

			Vector2Integer constraintsSize = new Vector2Integer(Items.Equal(constraintsRange.Start.RowCount, constraintsRange.End.RowCount), Items.Equal(constraintsRange.Start.ColumnCount, constraintsRange.End.ColumnCount));
			if (constraintsSize.Y != 1) throw new ArgumentException("Parameter constraintsRange is not a row vector range.");
			this.constraintsSize = constraintsSize.X;

			IntPtr x_L = positionRange.Start.Rows.Single().Copy();
			IntPtr x_U = positionRange.End.Rows.Single().Copy();
			IntPtr g_L = constraintsRange.Start.Rows.Single().Copy();
			IntPtr g_U = constraintsRange.End.Rows.Single().Copy();
			int nele_jac = constraintsSize.X * positionSize.X;
			int nele_hess = positionSize.X * positionSize.X;

			this.problemHandle = Wrapper.CreateIpoptProblem(positionSize.X, x_L, x_U, constraintsSize.X, g_L, g_U, nele_jac, nele_hess, 0, eval_f, eval_g, eval_grad_f, eval_jac_g, eval_h);

			Marshal.FreeCoTaskMem(x_L);
			Marshal.FreeCoTaskMem(x_U);
			Marshal.FreeCoTaskMem(g_L);
			Marshal.FreeCoTaskMem(g_U);

			instances.Add(problemHandle, this);
		}
		~Problem()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				instances.Remove(problemHandle);
				Wrapper.FreeIpoptProblem(problemHandle);
			}
		}

		public Matrix Solve(Matrix startPosition)
		{
			if (startPosition.RowCount != positionSize) throw new ArgumentException("Parameter 'startPosition' has the wrong row count.");
			if (startPosition.ColumnCount != 1) throw new ArgumentException("Parameter 'startPosition' is not a row vector.");

			IntPtr x = startPosition.Rows.Single().Copy();

			ApplicationReturnStatus returnStatus = Wrapper.IpoptSolve(problemHandle, x, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, problemHandle);

			if (returnStatus != ApplicationReturnStatus.Solve_Succeeded) throw new InvalidOperationException(string.Format("Error while solving problem: {0}.", returnStatus.ToString()));

			IEnumerable<double> result = x.Read<double>(positionSize);
			Marshal.FreeCoTaskMem(x);

			return Matrix.FromRowVectors(result.Select(Matrix.CreateSingleton));
		}

		static bool eval_f(int n, IntPtr x, bool new_x, IntPtr obj_value, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.positionSize).Select(Matrix.CreateSingleton));

			double value = problem.objective.GetValues(position).Single()[0, 0];

			Marshal.StructureToPtr(value, obj_value, false);

			return true;
		}
		static bool eval_grad_f(int n, IntPtr x, bool new_x, IntPtr grad_f, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.positionSize).Select(Matrix.CreateSingleton));

			Matrix gradient = problem.objective.GetGradients(position).Single();

			grad_f.Write(gradient.Columns.Single());

			return true;
		}
		static bool eval_g(int n, IntPtr x, bool new_x, int m, IntPtr g, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.positionSize).Select(Matrix.CreateSingleton));

			IEnumerable<double> values = problem.constraints.GetValues(position).Select(value => value[0, 0]);

			g.Write(values);

			return true;
		}
		static bool eval_jac_g(int n, IntPtr x, bool new_x, int m, int nele_jac, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.positionSize).Select(Matrix.CreateSingleton));

			Matrix constraintsJacobian = Matrix.FromRowVectors(problem.constraints.GetGradients(position).Select(gradient => gradient.Transpose));

			var entries =
				from rowIndex in Enumerable.Range(0, constraintsJacobian.RowCount)
				from columnIndex in Enumerable.Range(0, constraintsJacobian.ColumnCount)
				select new { RowIndex = rowIndex, ColumnIndex = columnIndex, Value = constraintsJacobian[rowIndex, columnIndex] };

			iRow.Write(entries.Select(entry => entry.RowIndex));
			jCol.Write(entries.Select(entry => entry.ColumnIndex));
			values.Write(entries.Select(entry => entry.Value));

			return true;
		}
		static bool eval_h(int n, IntPtr x, bool new_x, double obj_factor, int m, IntPtr lambda, bool new_lambda, int nele_hess, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.positionSize).Select(Matrix.CreateSingleton));
			double objectiveFactor = obj_factor;
			IEnumerable<double> constraintFactors = lambda.Read<double>(problem.constraintsSize);

			Matrix objectiveHessian = objectiveFactor * problem.objective.GetHessians(position).Single();
			IEnumerable<Matrix> constraintHessians = Enumerable.Zip(constraintFactors, problem.constraints.GetHessians(position), (factor, matrix) => factor * matrix);

			Matrix result = objectiveHessian + constraintHessians.Aggregate(new Matrix(problem.positionSize, problem.positionSize), (current, matrix) => current + matrix);

			var entries =
				from rowIndex in Enumerable.Range(0, result.RowCount)
				from columnIndex in Enumerable.Range(0, result.ColumnCount)
				select new { RowIndex = rowIndex, ColumnIndex = columnIndex, Value = result[rowIndex, columnIndex] };

			iRow.Write(entries.Select(entry => entry.RowIndex));
			jCol.Write(entries.Select(entry => entry.ColumnIndex));
			values.Write(entries.Select(entry => entry.Value));

			return true;
		}
	}
}

