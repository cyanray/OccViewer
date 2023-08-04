using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace OccViewer.Viewer
{
    /// <summary>
    /// Tool object for output OCCT rendering with Direct3D.
    /// </summary>
    class D3DViewer
    {
        /// <summary> Direct3D output image. </summary>
        private readonly D3DImage m_D3DImage = new();

        /// <summary> Direct3D color surface. </summary>
        private IntPtr m_ColorSurf;

        private bool m_IsFailed = false;

        public OCCViewer? Viewer { get; private set; }

        public D3DImage Image => m_D3DImage;

        /// <summary> Creates new Direct3D-based OCCT viewer. </summary>
        public D3DViewer()
        {
            m_D3DImage.IsFrontBufferAvailableChanged
              += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            BeginRenderingScene();
        }

        /// <summary> Creates new Direct3D-based OCCT viewer. </summary>
        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_D3DImage.IsFrontBufferAvailable)
            {
                // If the front buffer is available, then WPF has just created a new
                // Direct3D device, thus we need to start rendering our custom scene
                BeginRenderingScene();
            }
            else
            {
                // If the front buffer is no longer available, then WPF has lost Direct3D
                // device, thus we need to stop rendering until the new device is created
                StopRenderingScene();
            }
        }

        /// <summary> Initializes Direct3D-OCCT rendering. </summary>
        private void BeginRenderingScene()
        {
            if (m_IsFailed) return;

            if (m_D3DImage.IsFrontBufferAvailable)
            {
                Viewer = new OCCViewer();

                if (!Viewer.InitViewer())
                {
                    // TODO: handle error
                    MessageBox.Show("Failed to initialize OpenGL-Direct3D interoperability!",
                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    m_IsFailed = true;
                    return;
                }

                // Leverage the Rendering event of WPF composition
                // target to update the our custom Direct3D scene
                CompositionTarget.Rendering += OnRendering;
            }
        }

        /// <summary> Releases Direct3D-OCCT rendering. </summary>
        public void StopRenderingScene()
        {
            // This method is called when WPF loses its Direct3D device,
            // so we should just release our custom Direct3D scene
            CompositionTarget.Rendering -= OnRendering;
            m_ColorSurf = IntPtr.Zero;
        }

        /// <summary> Performs Direct3D-OCCT rendering. </summary>
        private void OnRendering(object? sender, EventArgs e)
        {
            UpdateScene();
        }

        /// <summary> Performs Direct3D-OCCT rendering. </summary>
        private void UpdateScene()
        {
            if (!m_IsFailed
              && m_D3DImage.IsFrontBufferAvailable
              && m_ColorSurf != IntPtr.Zero
              && (m_D3DImage.PixelWidth != 0 && m_D3DImage.PixelHeight != 0)
              && Viewer != null)
            {
                m_D3DImage.Lock();
                {
                    // Update the scene (via a call into our custom library)
                    Viewer.View.RedrawView();

                    // Invalidate the updated region of the D3DImage
                    m_D3DImage.AddDirtyRect(new Int32Rect(0, 0, m_D3DImage.PixelWidth, m_D3DImage.PixelHeight));
                }
                m_D3DImage.Unlock();
            }
        }

        /// <summary> Resizes Direct3D surfaces and OpenGL FBO. </summary>
        public void Resize(int theSizeX, int theSizeY)
        {
            if (!m_IsFailed && m_D3DImage.IsFrontBufferAvailable && Viewer != null)
            {
                // Set the back buffer for Direct3D WPF image
                m_D3DImage.Lock();
                {
                    m_D3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                    m_ColorSurf = Viewer.View.ResizeBridgeFBO(theSizeX, theSizeY);
                    m_D3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, m_ColorSurf);
                }
                m_D3DImage.Unlock();
            }
        }
    }
}
