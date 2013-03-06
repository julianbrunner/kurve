using System;
using System.Collections.Generic;
using System.Linq;
using Krach.Basics;
using Krach.Extensions;
using System.Runtime.InteropServices;
using Krach.Design;
using Krach.Terms;
using Krach.Terms.Rewriting;

namespace Kurve.Ipopt
{
	public class Problem : IDisposable
	{
		static Dictionary<IntPtr, Problem> instances = new Dictionary<IntPtr, Problem>();
		
		readonly int parameterCount;
		readonly int constraintCount;
		readonly Function simplifiedObjective;
		readonly IEnumerable<Function> simplifiedObjectiveJacobian;
		readonly IEnumerable<IEnumerable<Function>> simplifiedObjectiveHessian;
		readonly IEnumerable<Function> simplifiedConstraints;
		readonly IEnumerable<IEnumerable<Function>> simplifiedConstraintJacobians;
		readonly IEnumerable<IEnumerable<IEnumerable<Function>>> simplifiedConstraintHessians;
		readonly IntPtr problemHandle;
		
		bool disposed = false;
		
		public Problem(Function objective, IEnumerable<Constraint> constraints, Settings settings, Rewriter simplifier)
		{
			if (objective == null) throw new ArgumentNullException("objective");
			if (constraints == null) throw new ArgumentNullException("constraints");
			if (settings == null) throw new ArgumentNullException("settings");
			if (simplifier == null) throw new ArgumentNullException("simplifier");
			
			IEnumerable<int> parameterCounts = 
				Enumerables.Concatenate(Enumerables.Create(objective), constraints.Select(constraint => constraint.Function))
			    .Select(function => function.ParameterCount).Distinct();
			
			if (parameterCounts.Count() != 1) throw new ArgumentException("Parameter count of objective and constraints do not match.");
			
			this.parameterCount = parameterCounts.Single();
			this.constraintCount = constraints.Count();
			this.simplifiedObjective = simplifier.Rewrite(objective);
			this.simplifiedObjectiveJacobian = 
			(
				from partialDerivative in simplifier.Rewrite(objective).GetJacobian()
				select simplifier.Rewrite(partialDerivative)
			)
			.ToArray();
			this.simplifiedObjectiveHessian = 
			(
				from partialDerivative1 in simplifier.Rewrite(objective).GetJacobian()
				select
				(
					from partialDerivative2 in simplifier.Rewrite(partialDerivative1).GetJacobian()
					select simplifier.Rewrite(partialDerivative2)
				)
				.ToArray()
			)
			.ToArray();
			this.simplifiedConstraints = 
			(
				from constraint in constraints
				select simplifier.Rewrite(constraint.Function)
			)
			.ToArray();
			this.simplifiedConstraintJacobians = 
			(
				from constraint in constraints
				select
				(
					from partialDerivative in simplifier.Rewrite(constraint.Function).GetJacobian()
					select simplifier.Rewrite(partialDerivative)
				)
				.ToArray()
			)
			.ToArray();
			this.simplifiedConstraintHessians = 
			(
				from constraint in constraints
				select
				(
					from partialDerivative1 in simplifier.Rewrite(constraint.Function).GetJacobian()
					select
					(
						from partialDerivative2 in simplifier.Rewrite(partialDerivative1).GetJacobian()
						select simplifier.Rewrite(partialDerivative2)
					)
					.ToArray()
				)
				.ToArray()
			)
			.ToArray();
			
			IntPtr x_L = Enumerable.Repeat(-1e20, parameterCount).Copy();
			IntPtr x_U = Enumerable.Repeat(+1e20, parameterCount).Copy();
			IntPtr g_L = constraints.Select(constraint => constraint.Range.Start).Copy();
			IntPtr g_U = constraints.Select(constraint => constraint.Range.End).Copy();
			int nele_jac = constraintCount * parameterCount;
			int nele_hess = parameterCount * parameterCount;

			this.problemHandle = Wrapper.CreateIpoptProblem(parameterCount, x_L, x_U, constraintCount, g_L, g_U, nele_jac, nele_hess, 0, eval_f, eval_g, eval_grad_f, eval_jac_g, eval_h);

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

		public IEnumerable<double> Solve(IEnumerable<double> startPosition)
		{
			if (startPosition.Count() != parameterCount) throw new ArgumentException("Parameter 'startPosition' has the wrong count.");

			IntPtr x = startPosition.Copy();

			ApplicationReturnStatus returnStatus = Wrapper.IpoptSolve(problemHandle, x, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, problemHandle);

			if (returnStatus != ApplicationReturnStatus.Solve_Succeeded && returnStatus != ApplicationReturnStatus.Solved_To_Acceptable_Level)
				throw new InvalidOperationException(string.Format("Error while solving problem: {0}.", returnStatus.ToString()));

			IEnumerable<double> result = x.Read<double>(parameterCount);
			Marshal.FreeCoTaskMem(x);

			return result;
		}

		static bool eval_f(int n, IntPtr x, bool new_x, IntPtr obj_value, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			IEnumerable<double> position = x.Read<double>(problem.parameterCount);

			double value = problem.simplifiedObjective.Evaluate(position);

			Marshal.StructureToPtr(value, obj_value, false);

			return true;
		}
		static bool eval_grad_f(int n, IntPtr x, bool new_x, IntPtr grad_f, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			IEnumerable<double> position = x.Read<double>(problem.parameterCount);

			IEnumerable<double> jacobian = problem.simplifiedObjectiveJacobian.Select(objectPartialDerivative => objectPartialDerivative.Evaluate(position)).ToArray();

			grad_f.Write(jacobian);

			return true;
		}
		static bool eval_g(int n, IntPtr x, bool new_x, int m, IntPtr g, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			if (problem.constraintCount == 0) return true;

			IEnumerable<double> position = x.Read<double>(problem.parameterCount);

			IEnumerable<double> values = problem.simplifiedConstraints.Select(constraint => constraint.Evaluate(position));;

			g.Write(values);

			return true;
		}
		static bool eval_jac_g(int n, IntPtr x, bool new_x, int m, int nele_jac, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			if (problem.constraintCount == 0) return true;
			
			// values being null indicates that the structure of the jacobian should be returned.
			if (values == IntPtr.Zero)
			{
				var entries =
					from rowIndex in Enumerable.Range(0, problem.constraintCount)
					from columnIndex in Enumerable.Range(0, problem.parameterCount)
					select new { RowIndex = rowIndex, ColumnIndex = columnIndex };

				iRow.Write(entries.Select(entry => entry.RowIndex));
				jCol.Write(entries.Select(entry => entry.ColumnIndex));
			}
			// Otherwise, the jacobian is evaluated at the position.
			else
			{
				IEnumerable<double> position = x.Read<double>(problem.parameterCount);
	
				var entries =
					from constraintJacobian in problem.simplifiedConstraintJacobians
					from constraintPartialDerivative in constraintJacobian
					select constraintPartialDerivative.Evaluate(position);

				values.Write(entries);
			}

			return true;
		}
		static bool eval_h(int n, IntPtr x, bool new_x, double obj_factor, int m, IntPtr lambda, bool new_lambda, int nele_hess, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];
			
			// values being null indicates that the structure of the hessian should be returned.
			if (values == IntPtr.Zero)
			{
				var entries =
					from rowIndex in Enumerable.Range(0, problem.parameterCount)
					from columnIndex in Enumerable.Range(0, problem.parameterCount)
					select new { RowIndex = rowIndex, ColumnIndex = columnIndex };

				iRow.Write(entries.Select(entry => entry.RowIndex));
				jCol.Write(entries.Select(entry => entry.ColumnIndex));
			}
			else
			{
				Matrix objectiveHessian = new Matrix(problem.parameterCount, problem.parameterCount);
				IEnumerable<Matrix> constraintHessians = Enumerable.Empty<Matrix>();
				
				double objectiveFactor = obj_factor;
				if (objectiveFactor != 0)
				{
					IEnumerable<double> position = x.Read<double>(problem.parameterCount);
	
					objectiveHessian = objectiveFactor * Matrix.FromRowVectors
					(
						from objectiveJacobian in problem.simplifiedObjectiveHessian
						select Matrix.FromColumnVectors
						(
							from objectivePartialDerivative in objectiveJacobian
							select Matrix.CreateSingleton(objectivePartialDerivative.Evaluate(position))
					 	)   
					);
				}
				if (problem.constraintCount != 0)
				{
					IEnumerable<double> constraintFactors = lambda.Read<double>(problem.constraintCount);
					if (constraintFactors.Any(factor => factor != 0))
					{
						IEnumerable<double> position = x.Read<double>(problem.parameterCount);
	
						constraintHessians = 
							from constraintHessian in problem.simplifiedConstraintHessians
							select Matrix.FromRowVectors
							(
								from constraintJacobian in constraintHessian
								select Matrix.FromColumnVectors
								(
									from constraintPartialDerivative in constraintJacobian
									select Matrix.CreateSingleton(constraintPartialDerivative.Evaluate(position))
							 	)   
							);
						
						constraintHessians = Enumerable.Zip(constraintFactors, constraintHessians, (factor, matrix) => factor * matrix);
					}
				}

				Matrix result = 
					objectiveHessian + 
					constraintHessians.Aggregate(new Matrix(problem.parameterCount, problem.parameterCount), (current, matrix) => current + matrix);

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
	}
}

