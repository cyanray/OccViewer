#include "Viewer.h"
#using <System.dll>
#using <WindowsBase.dll>
#include <utility>
#include <vcclr.h>
#include <thread>
#include <Windows.h>

#pragma comment(lib, "Ole32.lib")

using namespace System;
using namespace System;
using namespace System::Windows;
using namespace OccViewer::Viewer;

namespace OccViewerProxy
{
	ref class EventReceiver
	{
	public:
		void OnClosing(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e)
		{
			e->Cancel = true;
			((Window^)sender)->Hide();
		}
	};

	class Viewer::Impl
	{
	public:
		Impl() :Window(gcnew MainWindow())
		{
			Viewer = Window->ActiveViewer;
		}
		gcroot<MainWindow^> Window;
		gcroot<OccViewer::Viewer::OCCViewer^> Viewer;
	};

	Viewer::Viewer()
	{
		HRESULT result = CoInitialize(NULL);
		m_pImpl = new Impl();
		EventReceiver^ er = gcnew EventReceiver();
		m_pImpl->Window->Closing += gcnew System::ComponentModel::CancelEventHandler(er, &EventReceiver::OnClosing);
	}

	Viewer::~Viewer()
	{
		delete m_pImpl;
	}

	void* Viewer::GetViewPtr() const
	{
		return m_pImpl->Viewer->GetViewPtr().ToPointer();
	}

	void* Viewer::GetAISContextPtr() const
	{
		return m_pImpl->Viewer->GetAISContextPtr().ToPointer();
	}

	void Viewer::ShowDialog()
	{
		m_pImpl->Window->ShowDialog();
	}
}


