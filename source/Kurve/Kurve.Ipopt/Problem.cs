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
		
		readonly IFunction objectiveFunction;
		readonly IFunction constraintFunction;
		readonly IntPtr problemHandle;
		
		bool disposed = false;
		
		int DomainDimension { get { return Items.Equal(objectiveFunction.DomainDimension, constraintFunction.DomainDimension); } }
		int ObjectiveDimension { get { return objectiveFunction.CodomainDimension; } }
		int ConstraintDimension { get { return constraintFunction.CodomainDimension; } }
		
		public Problem(IFunction objectiveFunction, Constraint constraint, Settings settings)
		{
			if (objectiveFunction == null) throw new ArgumentNullException("objectiveFunction");
			if (constraint == null) throw new ArgumentNullException("constraint");
			if (settings == null) throw new ArgumentNullException("settings");
			
			if (objectiveFunction.CodomainDimension != 1) throw new ArgumentException("The given objective function has a codomain dimension greater than 1.");
			if (objectiveFunction.DomainDimension != constraint.Function.DomainDimension) throw new ArgumentException("The domain dimensions of the objective and the constraint functions do not match.");
			
			this.objectiveFunction = objectiveFunction;
			this.constraintFunction = constraint.Function;
			
			IntPtr x_L = Enumerable.Repeat(-1e20, DomainDimension).Copy();
			IntPtr x_U = Enumerable.Repeat(+1e20, DomainDimension).Copy();
			IntPtr g_L = constraint.Ranges.Select(range => range.Start).Copy();
			IntPtr g_U = constraint.Ranges.Select(range => range.End).Copy();
			int nele_jac = ConstraintDimension * DomainDimension;
			int nele_hess = DomainDimension * DomainDimension;

			this.problemHandle = Wrapper.CreateIpoptProblem(DomainDimension, x_L, x_U, ConstraintDimension, g_L, g_U, nele_jac, nele_hess, 0, eval_f, eval_g, eval_grad_f, eval_jac_g, eval_h);

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
			if (startPosition.Count() != DomainDimension) throw new ArgumentException("Parameter 'startPosition' has the wrong item count.");

			IntPtr x = startPosition.Copy();

			ApplicationReturnStatus returnStatus = Wrapper.IpoptSolve(problemHandle, x, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, problemHandle);

			if (returnStatus != ApplicationReturnStatus.Solve_Succeeded && returnStatus != ApplicationReturnStatus.Solved_To_Acceptable_Level)
				throw new InvalidOperationException(string.Format("Error while solving problem: {0}.", returnStatus.ToString()));

			IEnumerable<double> result = x.Read<double>(DomainDimension);
			
			Marshal.FreeCoTaskMem(x);

			return result;
		}

		static bool eval_f(int n, IntPtr x, bool new_x, IntPtr obj_value, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			IEnumerable<double> position = x.Read<double>(problem.DomainDimension);

			double result = problem.objectiveFunction.Evaluate(position).Single();

			Marshal.StructureToPtr(result, obj_value, false);

			return true;
		}
		static bool eval_grad_f(int n, IntPtr x, bool new_x, IntPtr grad_f, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			IEnumerable<double> position = x.Read<double>(problem.DomainDimension);

			IEnumerable<double> result =
				from derivative1 in problem.objectiveFunction.GetDerivatives()
				select derivative1.Evaluate(position).Single();

			grad_f.Write(result);

			return true;
		}
		static bool eval_g(int n, IntPtr x, bool new_x, int m, IntPtr g, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			if (problem.ConstraintDimension == 0) return true;

			IEnumerable<double> position = x.Read<double>(problem.DomainDimension);

			IEnumerable<double> result = problem.constraintFunction.Evaluate(position);

			g.Write(result);

			return true;
		}
		static bool eval_jac_g(int n, IntPtr x, bool new_x, int m, int nele_jac, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data)
		{
			Problem problem = instances[user_data];

			if (problem.ConstraintDimension == 0) return true;
			
			// values being null indicates that the structure of the jacobian should be returned.
			if (values == IntPtr.Zero)
			{
				var entries =
					from columnIndex in Enumerable.Range(0, problem.DomainDimension)
					from rowIndex in Enumerable.Range(0, problem.ConstraintDimension)
					select new { RowIndex = rowIndex, ColumnIndex = columnIndex };

				iRow.Write(entries.Select(entry => entry.RowIndex));
				jCol.Write(entries.Select(entry => entry.ColumnIndex));
			}
			// otherwise, the jacobian is evaluated at the position.
			else
			{
				IEnumerable<double> position = x.Read<double>(problem.DomainDimension);
	
				IEnumerable<double> result =
					from derivative1 in problem.constraintFunction.GetDerivatives()
					from value in derivative1.Evaluate(position)
					select value;

				values.Write(result);
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
					from rowIndex in Enumerable.Range(0, problem.DomainDimension)
					from columnIndex in Enumerable.Range(0, problem.DomainDimension)
					select new { RowIndex = rowIndex, ColumnIndex = columnIndex };

				iRow.Write(entries.Select(entry => entry.RowIndex));
				jCol.Write(entries.Select(entry => entry.ColumnIndex));
			}
			else
			{
				Matrix objectiveHessian = new Matrix(problem.DomainDimension, problem.DomainDimension);
				Matrix constraintHessians = new Matrix(problem.DomainDimension, problem.DomainDimension);
				
				double objectiveFactor = obj_factor;
				if (objectiveFactor != 0)
				{
					IEnumerable<double> position = x.Read<double>(problem.DomainDimension);
	
					objectiveHessian = objectiveFactor * Matrix.FromColumnVectors
					(
						from derivative1 in problem.objectiveFunction.GetDerivatives()
						select Matrix.FromRowVectors
						(
							from derivative2 in derivative1.GetDerivatives()
							select Matrix.CreateSingleton(derivative2.Evaluate(position).Single())
					 	)   
					);
				}
				if (problem.ConstraintDimension != 0)
				{
					IEnumerable<double> constraintFactors = lambda.Read<double>(problem.ConstraintDimension);
					if (constraintFactors.Any(factor => factor != 0))
					{
						IEnumerable<double> position = x.Read<double>(problem.DomainDimension);
	
						constraintHessians = Matrix.FromColumnVectors
						(
							from derivative1 in problem.constraintFunction.GetDerivatives()
							select Matrix.FromRowVectors
							(
								from derivative2 in derivative1.GetDerivatives()
								let derivative2Values = derivative2.Evaluate(position)
								let combination = Enumerable.Zip(constraintFactors, derivative2Values, (factor, value) => factor * value).Sum()
								select Matrix.CreateSingleton(combination)
						 	)   
						);
					}
				}

				Matrix hessian = objectiveHessian + constraintHessians;

				IEnumerable<double> result =
					from rowIndex in Enumerable.Range(0, hessian.RowCount)
					from columnIndex in Enumerable.Range(0, hessian.ColumnCount)
					select hessian[rowIndex, columnIndex];

				values.Write(result);
			}

 			return true;
		}
		static bool intermediate_cb(int alg_mod, int iter_count, double obj_value, double inf_pr, double inf_du, double mu, double d_norm, double regularization_size, double alpha_du, double alpha_pr, int ls_trials, IntPtr user_data)
		{
			return true;
		}
	}
}

