#include "Viewer.h"
#using <System.dll>
#using <WindowsBase.dll>
using namespace OccViewer::Viewer;
using namespace System;
using namespace System::Windows;

namespace OccViewerProxy
{
	public ref class MyApplication : public Application
	{
	};


	void Viewer::ShowDialog()
	{
		MainWindow^ window = gcnew MainWindow();
		MyApplication^ app = gcnew MyApplication();
		app->Run(window);
	}
}


