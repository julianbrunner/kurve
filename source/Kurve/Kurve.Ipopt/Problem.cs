using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;

namespace Kurve.Ipopt
{
	public abstract class Problem : IDisposable
	{
		static Dictionary<IntPtr, Problem> instances = new Dictionary<IntPtr, Problem>();

		readonly IntPtr problemHandle;
		readonly int positionSize;
		readonly int constraintsSize;
		
		bool disposed = false;

		public abstract double EvaluateObjectiveValue(Matrix position);
		public abstract Matrix EvaluateObjectiveGradient(Matrix position);
		public abstract Matrix EvaluateObjectiveHessian(Matrix position, double objectiveFactor, Matrix constraintMultipliers);
		public abstract Matrix EvaluateConstraintsValue(Matrix position);
		public abstract Matrix EvaluateConstraintsJacobian(Matrix position);

		public Problem(Range<Matrix> positionRange, Range<Matrix> constraintsRange)
		{
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

			return Matrices.ValuesToMatrix(result);
		}

		static bool eval_f(int n, IntPtr x, bool new_x, IntPtr obj_value, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrices.ValuesToMatrix(x.Read<double>(problem.positionSize));

			double value = problem.EvaluateObjectiveValue(position);

			Marshal.StructureToPtr(value, obj_value, false);

			return true;
		}
		static bool eval_grad_f(int n, IntPtr x, bool new_x, IntPtr grad_f, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrices.ValuesToMatrix(x.Read<double>(problem.positionSize));

			Matrix gradient = problem.EvaluateObjectiveGradient(position);

			if (gradient.RowCount != problem.positionSize) throw new InvalidOperationException("Result from call to EvaluateConstraintsValue has wrong row count.");
			if (gradient.ColumnCount != 1) throw new InvalidOperationException("Result from call to EvaluateConstraintsValue is not a row vector.");

			grad_f.Write(Matrices.MatrixToValues(gradient));

			return true;
		}
		static bool eval_h(int n, IntPtr x, bool new_x, double obj_factor, int m, IntPtr lambda, bool new_lambda, int nele_hess, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrices.ValuesToMatrix(x.Read<double>(problem.positionSize));
			double objectiveFactor = obj_factor;
			Matrix constraintMultipliers = Matrices.ValuesToMatrix(lambda.Read<double>(problem.constraintsSize));

			Matrix objectiveHessian = problem.EvaluateObjectiveHessian(position, objectiveFactor, constraintMultipliers);

			if (objectiveHessian.RowCount != problem.positionSize) throw new InvalidOperationException("Result from call to EvaluateObjectiveHessian has wrong row count.");
			if (objectiveHessian.ColumnCount != problem.positionSize) throw new InvalidOperationException("Result from call to EvaluateObjectiveHessian has wrong column count.");

			var entries =
				from rowIndex in Enumerable.Range(0, objectiveHessian.RowCount)
				from columnIndex in Enumerable.Range(0, objectiveHessian.ColumnCount)
				select new { RowIndex = rowIndex, ColumnIndex = columnIndex, Value = objectiveHessian[rowIndex, columnIndex] };

			iRow.Write(entries.Select(entry => entry.RowIndex));
			jCol.Write(entries.Select(entry => entry.ColumnIndex));
			values.Write(entries.Select(entry => entry.Value));

			return true;
		}
		static bool eval_g(int n, IntPtr x, bool new_x, int m, IntPtr g, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrices.ValuesToMatrix(x.Read<double>(problem.positionSize));

			Matrix constraints = problem.EvaluateConstraintsValue(position);

			if (constraints.RowCount != problem.constraintsSize) throw new InvalidOperationException("Result from call to EvaluateConstraintsValue has wrong row count.");
			if (constraints.ColumnCount != 1) throw new InvalidOperationException("Result from call to EvaluateConstraintsValue is not a row vector.");

			g.Write(Matrices.MatrixToValues(constraints));

			return true;
		}
		static bool eval_jac_g(int n, IntPtr x, bool new_x, int m, int nele_jac, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrices.ValuesToMatrix(x.Read<double>(problem.positionSize));

			Matrix constraintsJacobian = problem.EvaluateConstraintsJacobian(position);

			if (constraintsJacobian.RowCount != problem.constraintsSize) throw new InvalidOperationException("Result from call to EvaluateConstraintsJacobian has wrong row count.");
			if (constraintsJacobian.ColumnCount != problem.positionSize) throw new InvalidOperationException("Result from call to EvaluateConstraintsJacobian has wrong column count.");

			var entries =
				from rowIndex in Enumerable.Range(0, constraintsJacobian.RowCount)
				from columnIndex in Enumerable.Range(0, constraintsJacobian.ColumnCount)
				select new { RowIndex = rowIndex, ColumnIndex = columnIndex, Value = constraintsJacobian[rowIndex, columnIndex] };

			iRow.Write(entries.Select(entry => entry.RowIndex));
			jCol.Write(entries.Select(entry => entry.ColumnIndex));
			values.Write(entries.Select(entry => entry.Value));

			return true;
		}
	}
}

