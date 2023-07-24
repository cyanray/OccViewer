$env:OpenCASCADE_INSTALL_VC_VERSION="vc14"
# win64 or win32
$env:OpenCASCADE_INSTALL_ARCH="win64"
$env:OpenCASCADE_INSTALL_DIR=""
$env:OpenCASCADE_INSTALL_DIR_LIB= $env:OpenCASCADE_INSTALL_DIR + "\" + $env:OpenCASCADE_INSTALL_ARCH + "\" + $env:OpenCASCADE_INSTALL_VC_VERSION + "\"

Start-Process "./OccViewer.sln"