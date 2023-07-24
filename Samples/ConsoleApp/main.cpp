#include <iostream>
#include <Viewer.h>
#include <Windows.h>

#pragma comment(lib, "OccViewer.ViewerProxy.lib")

int main()
{
    HRESULT result = CoInitialize(NULL);
    std::cout << "Hello World!\n";
    OccViewerProxy::Viewer viewer;
    viewer.ShowDialog();
}

