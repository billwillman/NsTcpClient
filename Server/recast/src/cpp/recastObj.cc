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

	const long RCN_NAVMESH_VERSION = 1;

	struct rcnNavMeshSetHeader
	{
		long version;
		int tileCount;
		dtNavMeshParams params;
	};

	struct rcnNavMeshTileHeader
	{
		dtTileRef tileRef;
		int dataSize;
	};

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
		bool ret = false;
		if (node::Buffer::HasInstance(arg0))
		{
			auto len = node::Buffer::Length(arg0->ToObject());
			if (len > 0)
			{
				char* buf = node::Buffer::Data(arg0->ToObject());
				ret = obj->_LoadMapObj(buf, (int)len);
			}
		}
		Local<v8::Boolean> result = Nan::New(ret);
		info.GetReturnValue().Set(result);
	}




	bool recastObj::_LoadMapObj(const char* data, int dataSize)
	{
		// 加载OBJ网格
		auto state = dtnmBuildDTNavMeshFromRaw((const unsigned char*)data, dataSize, false, &m_NavMesh);
		return (state == DT_SUCCESS) && m_NavMesh;
	}


	recastObj::recastObj()
	{}

	recastObj::~recastObj()
	{
		if (m_NavMesh)
		{
			dtFreeNavMesh(m_NavMesh);
			m_NavMesh = nullptr;
		}
	}

	dtStatus recastObj::dtnmBuildDTNavMeshFromRaw(const unsigned char* data
		, int dataSize
		, bool safeStorage
		, dtNavMesh** ppNavMesh)
	{
		if (!data || dataSize < sizeof(rcnNavMeshSetHeader) || !ppNavMesh)
			return DT_FAILURE + DT_INVALID_PARAM;

		int pos = 0;
		int size = sizeof(rcnNavMeshSetHeader);

		rcnNavMeshSetHeader header;
		memcpy(&header, data, size);
		pos += size;

		if (header.version != RCN_NAVMESH_VERSION)
		{
			*ppNavMesh = 0;
			return DT_FAILURE + DT_WRONG_VERSION;
		}

		dtNavMesh* mesh = dtAllocNavMesh();
		if (!mesh)
		{
			*ppNavMesh = 0;
			return DT_FAILURE + DT_OUT_OF_MEMORY;
		}

		dtStatus status = mesh->init(&header.params);
		if (dtStatusFailed(status))
		{
			*ppNavMesh = 0;
			return status;
		}

		// Read tiles.
		bool success = true;
		for (int i = 0; i < header.tileCount; ++i)
		{
			rcnNavMeshTileHeader tileHeader;
			size = sizeof(rcnNavMeshTileHeader);
			memcpy(&tileHeader, &data[pos], size);
			pos += size;

			size = tileHeader.dataSize;
			if (!tileHeader.tileRef || !tileHeader.dataSize)
			{
				success = false;
				status = DT_FAILURE + DT_INVALID_PARAM;
				break;
			}

			unsigned char* tileData =
				(unsigned char*)dtAlloc(size, DT_ALLOC_PERM);
			if (!tileData)
			{
				success = false;
				status = DT_FAILURE + DT_OUT_OF_MEMORY;
				break;
			}
			memcpy(tileData, &data[pos], size);
			pos += size;

			status = mesh->addTile(tileData
				, size
				, (safeStorage ? DT_TILE_FREE_DATA : 0)
				, tileHeader.tileRef
				, 0);

			if (dtStatusFailed(status))
			{
				success = false;
				break;
			}
		}

		if (!success)
		{
			dtFreeNavMesh(mesh);
			*ppNavMesh = 0;
			return status;
		}

		*ppNavMesh = mesh;

		return DT_SUCCESS;

	}
}