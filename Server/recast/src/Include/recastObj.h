#pragma once

#include <nan.h>
#include <nan_object_wrap.h>

namespace recast
{
	// recast对象
	class recastObj : public Nan::ObjectWrap
	{
		
	public:
		// 模块注册初始化
		static NAN_MODULE_INIT(Init);
	private:
		bool _LoadMapObj(const char* fileName);
	private:
		explicit recastObj();
		~recastObj();

		/* 导出方法 */
		static NAN_METHOD(New);
		static NAN_METHOD(Release);
		// 加载导航网格地图
		static NAN_METHOD(LoadMapObj);
		/*----------*/

		static Nan::Persistent<v8::Function> constructor;
	};
}