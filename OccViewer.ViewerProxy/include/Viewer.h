#pragma once
#pragma unmanaged

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
		void ShowDialog();
	};
}

#pragma managed