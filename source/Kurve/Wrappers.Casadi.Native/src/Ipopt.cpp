#include <cstring>
#include <symbolic/casadi.hpp>
#include <interfaces/ipopt/ipopt_solver.hpp>

using namespace std;
using namespace CasADi;

extern "C"
{
	const IpoptSolver* IpoptSolverCreate(SXFunction* objectiveFunction, SXFunction* constraintFunction)
	{
		return new IpoptSolver(*objectiveFunction, *constraintFunction);
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
	void IpoptSolverDispose(IpoptSolver* solver)
	{
		delete solver;
	}

	void SetBooleanOption(FX* function, const char* name, bool value)
	{
		function->setOption(string(name), value);
	}
	void SetIntegerOption(FX* function, const char* name, int value)
	{
		function->setOption(string(name), value);
	}
	void SetDoubleOption(FX* function, const char* name, double value)
	{
		function->setOption(string(name), value);
	}
	void SetStringOption(FX* function, const char* name, const char* value)
	{
		function->setOption(string(name), string(value));
	}
}
