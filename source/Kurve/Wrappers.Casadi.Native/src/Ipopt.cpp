#include <cstring>
#include <symbolic/casadi.hpp>
#include <interfaces/ipopt/ipopt_solver.hpp>

using namespace std;
using namespace CasADi;

struct IpoptProblem
{
	SXFunction F;
	SXFunction G;
	SXFunction H;
	SXFunction J;
	SXFunction GF;
};

extern "C"
{
	const IpoptProblem* IpoptProblemCreate(SXFunction* objectiveFunction, SXFunction* constraintFunction)
	{
		SXMatrix position = objectiveFunction->inputExpr(0);
		SXMatrix sigma = ssym("sigma", objectiveFunction->getNumScalarOutputs());
		SXMatrix lambda = ssym("lambda", constraintFunction->getNumScalarOutputs());

		vector<SXMatrix> lagrangeVariables;
		lagrangeVariables.push_back(position);
		lagrangeVariables.push_back(lambda);
		lagrangeVariables.push_back(sigma);

		SXMatrix lagrangeValue = sigma * objectiveFunction->eval(position) + inner_prod(lambda, constraintFunction->eval(position));
		SXFunction lagrangeFunction = SXFunction(lagrangeVariables, lagrangeValue);
		lagrangeFunction.init();

		IpoptProblem* problem = new IpoptProblem();

		problem->F = *objectiveFunction;
		problem->G = *constraintFunction;
		problem->H = SXFunction(lagrangeFunction.hessian());
		problem->J = SXFunction(constraintFunction->jacobian());
		problem->GF = SXFunction(objectiveFunction->gradient());

		return problem;
	}
	void IpoptProblemDispose(IpoptProblem* problem)
	{
		delete problem;
	}
	const IpoptProblem* IpoptProblemSubstitute(IpoptProblem* problem, SXMatrix* variable, SXMatrix* value)
	{
		IpoptProblem* newProblem = new IpoptProblem();

		newProblem->F = SXFunction(problem->F.inputExpr(), substitute(problem->F.outputExpr(0), *variable, *value));
		newProblem->G = SXFunction(problem->G.inputExpr(), substitute(problem->G.outputExpr(0), *variable, *value));
		newProblem->H = SXFunction(problem->H.inputExpr(), substitute(problem->H.outputExpr(0), *variable, *value));
		newProblem->J = SXFunction(problem->J.inputExpr(), substitute(problem->J.outputExpr(0), *variable, *value));
		newProblem->GF = SXFunction(problem->GF.inputExpr(), substitute(problem->GF.outputExpr(0), *variable, *value));

		return newProblem;
	}

	const IpoptSolver* IpoptSolverCreateSimple(SXFunction* objectiveFunction, SXFunction* constraintFunction)
	{
		return new IpoptSolver(*objectiveFunction, *constraintFunction);
	}
	const IpoptSolver* IpoptSolverCreate(IpoptProblem* problem)
	{
		return new IpoptSolver(problem->F, problem->G, problem->H, problem->J, problem->GF);
	}
	void IpoptSolverDispose(IpoptSolver* solver)
	{
		delete solver;
	}
	void IpoptSolverInitialize(IpoptSolver* solver)
	{
		solver->init();
	}
	void IpoptSolverSetConstraintBounds(IpoptSolver* solver, double* constraintLowerBounds, double* constraintUpperBounds, int constraintCount)
	{
		vector<double> constraintLowerBoundsValues;
		vector<double> constraintUpperBoundsValues;

		for (int index = 0; index < constraintCount; index++)
		{
			constraintLowerBoundsValues.push_back(constraintLowerBounds[index]);
			constraintUpperBoundsValues.push_back(constraintUpperBounds[index]);
		}

		solver->setInput(constraintLowerBoundsValues, NLP_LBG);
		solver->setInput(constraintUpperBoundsValues, NLP_UBG);
	}
	void IpoptSolverSetInitialPosition(IpoptSolver* solver, double* position, int positionCount)
	{
		vector<double> positionValues;

		for (int index = 0; index < positionCount; index++) positionValues.push_back(position[index]);

		solver->setInput(positionValues, NLP_X_INIT);
	}
	void IpoptSolverSolve(IpoptSolver* solver)
	{
		solver->solve();
	}
	void IpoptSolverGetResultPosition(IpoptSolver* solver, double* position, int positionCount)
	{
		vector<double> positionValues = vector<double>(positionCount);
		solver->getOutput(positionValues, NLP_X_OPT);
		memcpy(position, positionValues.data(), sizeof(double) * positionCount);
	}
}
