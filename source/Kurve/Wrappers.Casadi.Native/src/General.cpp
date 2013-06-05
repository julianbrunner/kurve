#include <cstring>
#include <symbolic/casadi.hpp>

using namespace std;
using namespace CasADi;

extern "C"
{
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
