#pragma once

#if defined(OccViewerProxy_LIB_EXPORT)
#pragma unmanaged
#endif

#if defined(OccViewerProxy_LIB_EXPORT)
#define EXPORTED __declspec(dllexport)
#else
#define EXPORTED __declspec(dllimport)
#endif

namespace OccViewerProxy
{
	class EXPORTED Viewer
	{
	public:
		Viewer();
		~Viewer();

		void* GetViewPtr() const;

		void* GetAISContextPtr() const;

		void ShowDialog();

	private:
		class Impl;
		Impl* m_pImpl;	
	};
}

#if defined(OccViewerProxy_LIB_EXPORT)
#pragma managed
#endif