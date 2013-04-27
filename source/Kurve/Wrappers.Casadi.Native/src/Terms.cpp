#include <cstring>
#include <symbolic/casadi.hpp>

using namespace std;
using namespace CasADi;

extern "C"
{
	SXMatrix* CreateValue(SXMatrix value)
	{
		return new SXMatrix(value);
	}
	SXFunction* CreateFunction(SXFunction function)
	{
		return new SXFunction(function);
	}

	void DisposeValue(SXMatrix* value)
	{
		delete value;
	}
	void DisposeFunction(SXFunction* function)
	{
		delete function;
	}

	SXMatrix* Variable(const char* name, int dimension)
	{
		return CreateValue(ssym(string(name), dimension));
	}
	SXFunction* Abstraction(SXMatrix* variable, SXMatrix* value)
	{
		SXFunction function = SXFunction(*variable, *value);

		function.init();

		return CreateFunction(function);
	}
	SXMatrix* Application(SXFunction* function, SXMatrix* value)
	{
		return CreateValue(function->eval(*value));
	}

	SXMatrix* Vector(SXMatrix** values, int valueCount)
	{
		SXMatrix result;

		for (int index = 0; index < valueCount; index++) result.append(*values[index]);

		return CreateValue(result);
	}
	SXMatrix* Selection(SXMatrix* value, int index)
	{
		return CreateValue(value->elem(index));
	}

	SXMatrix* Constant(double value)
	{
		return CreateValue(value);
	}

	SXMatrix* Sum(SXMatrix* value1, SXMatrix* value2)
	{
		return CreateValue(*value1 + *value2);
	}
	SXMatrix* Product(SXMatrix* value1, SXMatrix* value2)
	{
		return CreateValue(*value1 * *value2);
	}
	SXMatrix* Exponentiation(SXMatrix* value1, SXMatrix* value2)
	{
		return CreateValue(pow(*value1, *value2));
	}
	SXMatrix* MatrixProduct(SXMatrix* value1, SXMatrix* value2)
	{
		return CreateValue(value1->mul(*value2));
	}
	SXMatrix* Transpose(SXMatrix* value)
	{
		return CreateValue(trans(*value));
	}

	const char* ValueToString(SXMatrix* value)
	{
		string description = value->getDescription();
		char* result = new char[description.size() + 1];
		strcpy(result, description.c_str());

		return result;
	}
	int ValueDimension(SXMatrix* value)
	{
		return value->size1();
	}
	void ValueEvaluate(SXMatrix* value, double* values)
	{
		for (int rowIndex = 0; rowIndex < value->size1(); rowIndex++) values[rowIndex] = value->elem(rowIndex, 0).getValue();
	}
	const SXMatrix* ValueSimplify(SXMatrix* value)
	{
		SXMatrix result = *value;

		simplify(result);

		return CreateValue(result);
	}

	const char* FunctionToString(SXFunction* function)
	{
		stringstream descriptionStream;

		descriptionStream << "(λ " << function->inputExpr(0).getDescription() << ". " << function->outputExpr(0).getDescription() << ")";

		string description = descriptionStream.str();
		char* result = new char[description.size() + 1];
		strcpy(result, description.c_str());

		return result;
	}
	int FunctionDomainDimension(SXFunction* function)
	{
		return function->getNumScalarInputs();
	}
	int FunctionCodomainDimension(SXFunction* function)
	{
		return function->getNumScalarOutputs();
	}
	void FunctionDerivatives(SXFunction* function, SXFunction** derivatives)
	{
		FX jacobian = function->jacobian();

		jacobian.init();

		SXMatrix jacobianValue = jacobian.eval(function->inputExpr(0));

		for (int columnIndex = 0; columnIndex < jacobianValue.size2(); columnIndex++)
		{
			vector<SX> jacobianColumnValues;

			for (int rowIndex = 0; rowIndex < jacobianValue.size1(); rowIndex++) jacobianColumnValues.push_back(jacobianValue.elem(rowIndex, columnIndex));

			SXFunction derivative = SXFunction(function->inputExpr(0), SXMatrix(jacobianColumnValues));

			derivative.init();

			derivatives[columnIndex] = CreateFunction(derivative);
		}
	}
	const SXFunction* FunctionSimplify(SXFunction* function)
	{
		SXMatrix variable = function->inputExpr(0);
		SXMatrix value = function->outputExpr(0);

		simplify(value);

		SXFunction result = SXFunction(variable, value);

		result.init();

		return CreateFunction(result);
	}
}
