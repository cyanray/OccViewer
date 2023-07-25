#include "Viewer.h"
#using <System.dll>
#using <WindowsBase.dll>
#include <utility>
#include <vcclr.h>
using namespace System;
using namespace System;
using namespace System::Windows;
using namespace OccViewer::Viewer;

namespace OccViewerProxy
{
	public ref class MyApplication : public Application
	{
	};

	class Viewer::Impl
	{
	public:
		Impl() :Window(gcnew MainWindow()), App(gcnew MyApplication())
		{
			Viewer = Window->ActiveViewer;
		}
		gcroot<MyApplication^> App;
		gcroot<MainWindow^> Window;
		gcroot<OccViewer::Viewer::OCCViewer^> Viewer;
	};

	Viewer::Viewer() :m_pImpl(new Impl())
	{
	}

	Viewer::~Viewer()
	{
		delete m_pImpl;
	}

	void* Viewer::GetViewPtr()
	{
		return m_pImpl->Viewer->GetViewPtr().ToPointer();
	}

	void* Viewer::GetAISContextPtr()
	{
		return m_pImpl->Viewer->GetAISContextPtr().ToPointer();
	}

	void Viewer::ShowDialog()
	{
		m_pImpl->App->Run(m_pImpl->Window);
	}
}


