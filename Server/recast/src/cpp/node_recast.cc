#include "recastObj.h"

namespace recast
{
	using v8::Local;
	using v8::Object;

	void InitModule(Local<Object> exports)
	{
		recastObj::Init(exports);
	}

	NODE_MODULE(kcp, InitModule)
}