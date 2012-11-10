using System;
using System.Runtime.InteropServices;

namespace Kurve.Ipopt
{
	delegate bool ObjectiveValueEvaluator(int n, IntPtr x, bool new_x, IntPtr obj_value, IntPtr user_data);
	delegate bool ObjectiveGradientEvaluator(int n, IntPtr x, bool new_x, IntPtr grad_f, IntPtr user_data);
	delegate bool ObjectiveHessianEvaluator(int n, IntPtr x, bool new_x, double obj_factor, int m, IntPtr lambda, bool new_lambda, int nele_hess, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data);
	delegate bool ConstraintsValueEvaluator(int n, IntPtr x, bool new_x, int m, IntPtr g, IntPtr user_data);
	delegate bool ConstraintsJacobianEvaluator(int n, IntPtr x, bool new_x, int m, int nele_jac, IntPtr iRow, IntPtr jCol, IntPtr values, IntPtr user_data);
	// bool intermediate_cb(int alg_mod, int iter_count, double obj_value, double inf_pr, double inf_du, double mu, double d_norm, double regularization_size, double alpha_du, double alpha_pr, int ls_trials, IntPtr user_data);
	static class Wrapper
	{
		[DllImport("Ipopt")]
		public static extern IntPtr CreateIpoptProblem(int n, IntPtr x_L, IntPtr x_U, int m, IntPtr g_L, IntPtr g_U, int nele_jac, int nele_hess,
		                                               int index_style, ObjectiveValueEvaluator eval_f, ConstraintsValueEvaluator eval_g, ObjectiveGradientEvaluator eval_grad_f,
		                                               ConstraintsJacobianEvaluator eval_jac_g, ObjectiveHessianEvaluator eval_h);
		[DllImport("Ipopt")]
		public static extern void FreeIpoptProblem(IntPtr ipopt_problem);
		[DllImport("Ipopt")]
		public static extern ApplicationReturnStatus IpoptSolve(IntPtr ipopt_problem, IntPtr x, IntPtr g, IntPtr obj_val, IntPtr mult_g, IntPtr mult_x_L, IntPtr mult_x_U, IntPtr user_data);
	}
}

