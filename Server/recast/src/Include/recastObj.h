#pragma once

#include <nan.h>
#include <nan_object_wrap.h>
#include "DetourNavMesh.h"

namespace recast
{
	// recast����
	class recastObj : public Nan::ObjectWrap
	{
		
	public:
		// ģ��ע���ʼ��
		static NAN_MODULE_INIT(Init);
	private:
		bool _LoadMapObj(const char* data, int dataSize);
		static dtStatus dtnmBuildDTNavMeshFromRaw(const unsigned char* data
			, int dataSize
			, bool safeStorage
			, dtNavMesh** ppNavMesh);
	private:
		explicit recastObj();
		~recastObj();

		/* �������� */
		static NAN_METHOD(New);
		static NAN_METHOD(Release);
		// ���ص��������ͼ
		static NAN_METHOD(LoadMapObj);
		/*----------*/
		dtNavMesh* m_NavMesh;

		static Nan::Persistent<v8::Function> constructor;
	};
}