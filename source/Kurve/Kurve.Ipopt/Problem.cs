using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;
using Krach.Calculus;

namespace Kurve.Ipopt
{
	public class Problem : IDisposable
	{
		static Dictionary<IntPtr, Problem> instances = new Dictionary<IntPtr, Problem>();

		readonly Function objective;
		readonly Function constraints;
		readonly IntPtr problemHandle;
		
		bool disposed = false;

		public Problem(Function objective, Settings settings) : this(new DomainConstrainedFunction(objective, CreateUnboundedConstraints(objective.DomainDimension)), settings) { }
		public Problem(DomainConstrainedFunction objective, Settings settings)
		{
			if (objective == null) throw new ArgumentNullException("objective");
			if (settings == null) throw new ArgumentNullException("settings");

			this.objective = objective.Function;

			IntPtr x_L = objective.Constraints.Start.Columns.Single().Copy();
			IntPtr x_U = objective.Constraints.End.Columns.Single().Copy();
			IntPtr g_L = IntPtr.Zero;
			IntPtr g_U = IntPtr.Zero;
			int nele_jac = 0;
			int nele_hess = objective.Function.DomainDimension * objective.Function.DomainDimension;

			this.problemHandle = Wrapper.CreateIpoptProblem(objective.Function.DomainDimension, x_L, x_U, 0, g_L, g_U, nele_jac, nele_hess, 0, eval_f, eval_g, eval_grad_f, eval_jac_g, eval_h);

			settings.Apply(problemHandle);

			Wrapper.SetIntermediateCallback(problemHandle, intermediate_cb);

			Marshal.FreeCoTaskMem(x_L);
			Marshal.FreeCoTaskMem(x_U);

			instances.Add(problemHandle, this);
		}
		public Problem(Function objective, CodomainConstrainedFunction constraints, Settings settings): this(new DomainConstrainedFunction(objective, CreateUnboundedConstraints(objective.DomainDimension)), settings) { }
		public Problem(DomainConstrainedFunction objective, CodomainConstrainedFunction constraints, Settings settings)
		{
			if (objective == null) throw new ArgumentNullException("objective");
			if (constraints == null) throw new ArgumentNullException("constraints");
			if (settings == null) throw new ArgumentNullException("settings");

			if (objective.Function.DomainDimension != constraints.Function.DomainDimension) throw new ArgumentException("Domain dimension of objective and constraints functions do not match.");

			this.objective = objective.Function;
			this.constraints = constraints.Function;

			IntPtr x_L = objective.Constraints.Start.Columns.Single().Copy();
			IntPtr x_U = objective.Constraints.End.Columns.Single().Copy();
			IntPtr g_L = constraints.Constraints.Start.Columns.Single().Copy();
			IntPtr g_U = constraints.Constraints.End.Columns.Single().Copy();
			int nele_jac = constraints.Function.CodomainDimension * constraints.Function.DomainDimension;
			int nele_hess = objective.Function.DomainDimension * objective.Function.DomainDimension;

			this.problemHandle = Wrapper.CreateIpoptProblem(objective.Function.DomainDimension, x_L, x_U, constraints.Function.CodomainDimension, g_L, g_U, nele_jac, nele_hess, 0, eval_f, eval_g, eval_grad_f, eval_jac_g, eval_h);

			settings.Apply(problemHandle);

			Wrapper.SetIntermediateCallback(problemHandle, intermediate_cb);

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
			if (startPosition.RowCount != objective.DomainDimension) throw new ArgumentException("Parameter 'startPosition' has the wrong row count.");
			if (startPosition.ColumnCount != 1) throw new ArgumentException("Parameter 'startPosition' is not a row vector.");

			IntPtr x = startPosition.Columns.Single().Copy();

			ApplicationReturnStatus returnStatus = Wrapper.IpoptSolve(problemHandle, x, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, problemHandle);

			if (returnStatus != ApplicationReturnStatus.Solve_Succeeded && returnStatus != ApplicationReturnStatus.Solved_To_Acceptable_Level)
				throw new InvalidOperationException(string.Format("Error while solving problem: {0}.", returnStatus.ToString()));

			IEnumerable<double> result = x.Read<double>(objective.DomainDimension);
			Marshal.FreeCoTaskMem(x);

			return Matrix.FromRowVectors(result.Select(Matrix.CreateSingleton));
		}

		static bool eval_f(int n, IntPtr x, bool new_x, IntPtr obj_value, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.objective.DomainDimension).Select(Matrix.CreateSingleton));

			double value = problem.objective.GetValues(position).Single()[0, 0];

			Marshal.StructureToPtr(value, obj_value, false);

			return true;
		}
		static bool eval_grad_f(int n, IntPtr x, bool new_x, IntPtr grad_f, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.objective.DomainDimension).Select(Matrix.CreateSingleton));

			Matrix gradient = problem.objective.GetGradients(position).Single();

			grad_f.Write(gradient.Columns.Single());

			return true;
		}
		static bool eval_g(int n, IntPtr x, bool new_x, int m, IntPtr g, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			if (problem.constraints == null) return true;

			Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.objective.DomainDimension).Select(Matrix.CreateSingleton));

			IEnumerable<double> values = problem.constraints.GetValues(position).Select(value => value[0, 0]);

			g.Write(values);

			return true;
		}
		static bool eval_jac_g(int n, IntPtr x, bool new_x, int m, int nele_jac, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			if (problem.constraints == null) return true;

			if (values == IntPtr.Zero)
			{
				var entries =
					from rowIndex in Enumerable.Range(0, problem.constraints.CodomainDimension)
					from columnIndex in Enumerable.Range(0, problem.constraints.DomainDimension)
					select new { RowIndex = rowIndex, ColumnIndex = columnIndex };

				iRow.Write(entries.Select(entry => entry.RowIndex));
				jCol.Write(entries.Select(entry => entry.ColumnIndex));
			}
			else
			{
				Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.objective.DomainDimension).Select(Matrix.CreateSingleton));

				Matrix constraintsJacobian = Matrix.FromRowVectors(problem.constraints.GetGradients(position).Select(gradient => gradient.Transpose));

				var entries =
					from rowIndex in Enumerable.Range(0, problem.constraints.CodomainDimension)
					from columnIndex in Enumerable.Range(0, problem.constraints.DomainDimension)
					select new { Value = constraintsJacobian[rowIndex, columnIndex] };

				values.Write(entries.Select(entry => entry.Value));
			}

			return true;
		}
		static bool eval_h(int n, IntPtr x, bool new_x, double obj_factor, int m, IntPtr lambda, bool new_lambda, int nele_hess, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			if (values == IntPtr.Zero)
			{
				var entries =
					from rowIndex in Enumerable.Range(0, problem.objective.DomainDimension)
					from columnIndex in Enumerable.Range(0, problem.objective.DomainDimension)
					select new { RowIndex = rowIndex, ColumnIndex = columnIndex };

				iRow.Write(entries.Select(entry => entry.RowIndex));
				jCol.Write(entries.Select(entry => entry.ColumnIndex));
			}
			else
			{
				Option<Matrix> objectiveHessian = null;
				Option<IEnumerable<Matrix>> constraintHessians = null;
				
				double objectiveFactor = obj_factor;
				if (objectiveFactor != 0)
				{
					Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.objective.DomainDimension).Select(Matrix.CreateSingleton));

					objectiveHessian = new Option<Matrix>(objectiveFactor * problem.objective.GetHessians(position).Single());
				}
				if (problem.constraints != null)
				{
					IEnumerable<double> constraintFactors = lambda.Read<double>(problem.constraints.CodomainDimension);
					if (constraintFactors.Any(factor => factor != 0))
					{
						Matrix position = Matrix.FromRowVectors(x.Read<double>(problem.objective.DomainDimension).Select(Matrix.CreateSingleton));

						constraintHessians = new Option<IEnumerable<Matrix>>(Enumerable.Zip(constraintFactors, problem.constraints.GetHessians(position), (factor, matrix) => factor * matrix));
					}
				}

				Matrix result = new Matrix(problem.objective.DomainDimension, problem.objective.DomainDimension);
				if (objectiveHessian != null) result += objectiveHessian.Item;
				if (constraintHessians != null) result += constraintHessians.Item.Aggregate(new Matrix(problem.objective.DomainDimension, problem.objective.DomainDimension), (current, matrix) => current + matrix);

				var entries =
					from rowIndex in Enumerable.Range(0, result.RowCount)
					from columnIndex in Enumerable.Range(0, result.ColumnCount)
					select new { Value = result[rowIndex, columnIndex] };

				values.Write(entries.Select(entry => entry.Value));
			}

 			return true;
		}
		static bool intermediate_cb(int alg_mod, int iter_count, double obj_value, double inf_pr, double inf_du, double mu, double d_norm, double regularization_size, double alpha_du, double alpha_pr, int ls_trials, IntPtr user_data)
		{
			return true;
		}

		static Range<Matrix> CreateUnboundedConstraints(int size)
		{
			Matrix start = Matrix.FromRowVectors(Enumerable.Repeat(-1e20, size).Select(Matrix.CreateSingleton));
			Matrix end = Matrix.FromRowVectors(Enumerable.Repeat(+1e20, size).Select(Matrix.CreateSingleton));

			return new Range<Matrix>(start, end);
		}
	}
}

