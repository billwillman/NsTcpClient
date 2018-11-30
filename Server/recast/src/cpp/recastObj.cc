#include "recastObj.h"
#include "node_buffer.h"

#define string2char(string, len, out) \
    do {\
    int __i__ = 0;\
    for (; __i__ < len; __i__++) {\
        out[__i__] = (char)((*string)[__i__]);\
    }\
    } while(0)

namespace recast
{
	using v8::Context;
	using v8::Function;
	using Nan::FunctionCallbackInfo;
	using v8::FunctionTemplate;
	using v8::Local;
	using Nan::MaybeLocal;
	using v8::Number;
	using v8::Integer;
	using v8::Object;
	using Nan::Persistent;
	using v8::String;
	using v8::Value;
	using v8::Exception;
	using Nan::Null;
	using Nan::Callback;
	using Nan::GetFunction;
	using Nan::Set;
	using Nan::To;

	Persistent<Function> recastObj::constructor;

	NAN_MODULE_INIT(recastObj::Init)
	{
		Local<FunctionTemplate> tpl = Nan::New<FunctionTemplate>(New);
		// 设置JS的类名
		tpl->SetClassName(Nan::New("recastObj").ToLocalChecked());
		tpl->InstanceTemplate()->SetInternalFieldCount(1);

		SetPrototypeMethod(tpl, "release", Release);
		SetPrototypeMethod(tpl, "loadMapObj", LoadMapObj);


		constructor.Reset(GetFunction(tpl).ToLocalChecked());
		// 设置类名
		Set(target, Nan::New("recastObj").ToLocalChecked(), GetFunction(tpl).ToLocalChecked());
	}

	NAN_METHOD(recastObj::New)
	{
		if (info.IsConstructCall())
		{
			// 构造环境下创建
			recastObj* obj = new recastObj();
			obj->Wrap(info.This());
			info.GetReturnValue().Set(info.This());
		}
		else
		{
			Local<Function> cons = Nan::New(constructor);
			info.GetReturnValue().Set(Nan::NewInstance(cons, 0, NULL).ToLocalChecked());
		}
	}

	// 释放
	NAN_METHOD(recastObj::Release)
	{
		recastObj* thiz = ObjectWrap::Unwrap<recastObj>(info.Holder());
		delete thiz;
	}

	// 加载导航网格
	NAN_METHOD(recastObj::LoadMapObj)
	{
		recastObj* obj = ObjectWrap::Unwrap<recastObj>(info.Holder());
		// 字符串
		Local<Value> arg0 = info[0];
		if (arg0->IsString())
		{
			char* buf = NULL;
			// 必须是字符串
			String::Value fileName(arg0);
			int len = fileName.length();
			if (len == 0)
				return;
			if (!(buf = (char*)malloc(len))) {
				Nan::ThrowError("malloc error");
				return;
			}
			string2char(fileName, len, buf);
			bool ret = obj->_LoadMapObj(buf);
			free(buf);
			Local<v8::Boolean> result = Nan::New(ret);
			info.GetReturnValue().Set(result);
		}
	}




	bool recastObj::_LoadMapObj(const char* fileName)
	{
		// 加载OBJ网格
		return false;
	}


	recastObj::recastObj()
	{}

	recastObj::~recastObj()
	{}
}