#include <iostream>
#include <cstring>
#include <symbolic/casadi.hpp>
#include <interfaces/ipopt/ipopt_solver.hpp>
#include <symbolic/generic_type.hpp>

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

const string FunctionToString(SXFunction function)
{
	stringstream descriptionStream;

	descriptionStream << "(λ " << function.inputExpr(0).getDescription() << ". " << function.outputExpr(0).getDescription() << ")";

	return descriptionStream.str();
}

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
		SXMatrix lagrangeValue = sigma * objectiveFunction->eval(position) + (constraintFunction->getNumScalarOutputs() == 0 ? 0 : inner_prod(lambda, constraintFunction->eval(position)));
		SXFunction lagrangeFunction = SXFunction(lagrangeVariables, lagrangeValue);
		lagrangeFunction.init();

		IpoptProblem* problem = new IpoptProblem();

		problem->F = *objectiveFunction;
		problem->G = *constraintFunction;
		problem->H = SXFunction(lagrangeFunction.hessian());
		problem->J = constraintFunction->getNumScalarOutputs() == 0 ? SXFunction(position, SXMatrix(0, position.size1())) : SXFunction(constraintFunction->jacobian());
		problem->GF = SXFunction(objectiveFunction->gradient());

		return problem;
	}
	void IpoptProblemDispose(IpoptProblem* problem)
	{
		delete problem;
	}
//	const IpoptProblem* IpoptProblemSubstitute(IpoptProblem* problem, SXMatrix* variable, SXMatrix* value)
//	{
//		IpoptProblem* newProblem = new IpoptProblem();
//
//		newProblem->F = SXFunction(problem->F.inputExpr(), substitute(problem->F.outputExpr(0), *variable, *value));
//		newProblem->G = SXFunction(problem->G.inputExpr(), substitute(problem->G.outputExpr(0), *variable, *value));
//		newProblem->H = SXFunction(problem->H.inputExpr(), substitute(problem->H.outputExpr(0), *variable, *value));
//		newProblem->J = SXFunction(problem->J.inputExpr(), substitute(problem->J.outputExpr(0), *variable, *value));
//		newProblem->GF = SXFunction(problem->GF.inputExpr(), substitute(problem->GF.outputExpr(0), *variable, *value));
//
//		return newProblem;
//	}
	const IpoptProblem* IpoptProblemSubstitute(IpoptProblem* problem, SXMatrix** variables, SXMatrix** values, int count)
	{
		vector<SXMatrix> outputValuesVector;
		outputValuesVector.push_back(problem->F.outputExpr(0));
		outputValuesVector.push_back(problem->G.outputExpr(0));
		outputValuesVector.push_back(problem->H.outputExpr(0));
		outputValuesVector.push_back(problem->J.outputExpr(0));
		outputValuesVector.push_back(problem->GF.outputExpr(0));

		vector<SXMatrix> variablesVector;
		for (int index = 0; index < count; index++) variablesVector.push_back(*variables[index]);

		vector<SXMatrix> valuesVector;
		for (int index = 0; index < count; index++) valuesVector.push_back(*values[index]);

		outputValuesVector = substitute(outputValuesVector, variablesVector, valuesVector);

		IpoptProblem* newProblem = new IpoptProblem();

		newProblem->F = SXFunction(problem->F.inputExpr(), outputValuesVector[0]);
		newProblem->G = SXFunction(problem->G.inputExpr(), outputValuesVector[1]);
		newProblem->H = SXFunction(problem->H.inputExpr(), outputValuesVector[2]);
		newProblem->J = SXFunction(problem->J.inputExpr(), outputValuesVector[3]);
		newProblem->GF = SXFunction(problem->GF.inputExpr(), outputValuesVector[4]);

		return newProblem;
	}

	const IpoptSolver* IpoptSolverCreateSimple(SXFunction* objectiveFunction, SXFunction* constraintFunction)
	{
//		cout << "creating simple IpoptSolver..." << endl;
//		cout << "objective function: " << FunctionToString(*objectiveFunction) << endl;
//		cout << "constraint function: " << FunctionToString(*constraintFunction) << endl;

		return new IpoptSolver(*objectiveFunction, *constraintFunction);
	}
	const IpoptSolver* IpoptSolverCreate(IpoptProblem* problem)
	{
//		cout << "creating IpoptSolver..." << endl;
//		cout << "objective function: " << FunctionToString(problem->F) << endl;
//		cout << "constraint function: " << FunctionToString(problem->G) << endl;

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
	const char* IpoptSolverSolve(IpoptSolver* solver)
	{
		solver->solve();

		string returnStatus = solver->getStat("return_status").toString();
		char* result = new char[returnStatus.size() + 1];
		strcpy(result, returnStatus.c_str());

		return result;
	}
	void IpoptSolverGetResultPosition(IpoptSolver* solver, double* position, int positionCount)
	{
		vector<double> positionValues = vector<double>(positionCount);
		solver->getOutput(positionValues, NLP_X_OPT);
		memcpy(position, positionValues.data(), sizeof(double) * positionCount);
	}
}
