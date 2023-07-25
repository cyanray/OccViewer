using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Controls;

namespace OccViewer.Viewer
{
    public enum CurrentAction3d
    {
        Nothing,
        DynamicZooming,
        WindowZooming,
        DynamicPanning,
        GlobalPanning,
        DynamicRotation
    }
    public enum CurrentPressedKey
    {
        Nothing,
        Ctrl,
        Shift
    }
    public enum ModelFormat
    {
        BREP,
        STEP,
        IGES,
        VRML,
        STL,
        IMAGE
    }

    public enum DisplayMode
    {
        Wireframe,
        Shading
    }

    public class OCCViewer
    {
        public event EventHandler? ZoomingFinished;

        protected void RaiseZoomingFinished()
        {
            ZoomingFinished?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? AvaliabiltyOfOperationsChanged;

        protected void RaiseAvaliabiltyOfOperationsChanged()
        {
            AvaliabiltyOfOperationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public OCCTProxyD3D View { get; private set; }

        public CurrentAction3d CurrentMode { get; private set; }

        private bool IsRectVisible { get; set; }

        public bool DegenerateMode { get; private set; }


        public bool IsWireframeEnabled { get; private set; }

        public bool IsShadingEnabled { get; private set; }

        public bool IsRayTracingEnabled { get; private set; }

        public bool IsAntiAliasingEnabled { get; private set; }

        public bool IsTransparencyEnabled { get; private set; }

        public bool IsColorEnabled { get; private set; }

        public bool IsMaterialEnabled { get; private set; }

        public bool IsDeleteEnabled { get; private set; }

        public Color CurrentObjectColor
        {
            get
            {
                int r, g, b;
                r = View.GetObjColR();
                g = View.GetObjColG();
                b = View.GetObjColB();
                return Color.FromArgb(r, g, b);
            }
            set
            {
                View.SetColor(value.R, value.G, value.B);
                View.UpdateCurrentViewer();
            }
        }

        public Color BackgroundColor
        {
            get
            {
                int r, g, b;
                r = View.GetBGColR();
                g = View.GetBGColG();
                b = View.GetBGColB();
                return Color.FromArgb(r, g, b);
            }
            set
            {
                View.SetBackgroundColor(value.R, value.G, value.B);
                View.UpdateCurrentViewer();
            }
        }

        private float m_CurZoom;
        private int m_Xmin;
        private int m_Ymin;
        private int m_Xmax;
        private int m_Ymax;
        private int m_ButtonDownX;
        private int m_ButtonDownY;
        public OCCViewer()
        {
            View = new OCCTProxyD3D();
            View.InitOCCTProxy();
            CurrentMode = CurrentAction3d.Nothing;
            IsRectVisible = false;
            DegenerateMode = true;
        }

        public bool InitViewer()
        {
            return View.InitViewer();
        }

        public void ImportModel(ModelFormat theFormat)
        {
            int aFormat = 10;
            OpenFileDialog anOpenDialog = new();
            string aDataDir = Environment.GetEnvironmentVariable("CSF_OCCTDataPath");
            string aFilter = "";

            switch (theFormat)
            {
                case ModelFormat.BREP:
                    anOpenDialog.InitialDirectory = (aDataDir + "\\occ");
                    aFormat = 0;
                    aFilter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
                    break;
                case ModelFormat.STEP:
                    anOpenDialog.InitialDirectory = (aDataDir + "\\step");
                    aFormat = 1;
                    aFilter = "STEP Files (*.stp *.step)|*.stp; *.step";
                    break;
                case ModelFormat.IGES:
                    anOpenDialog.InitialDirectory = (aDataDir + "\\iges");
                    aFormat = 2;
                    aFilter = "IGES Files (*.igs *.iges)|*.igs; *.iges";
                    break;
                default:
                    break;
            }

            anOpenDialog.Filter = aFilter + "|All files (*.*)|*.*";
            if (anOpenDialog.ShowDialog() == DialogResult.OK)
            {
                string aFileName = anOpenDialog.FileName;
                if (aFileName == "")
                {
                    return;
                }

                if (!View.TranslateModel(aFileName, aFormat, true))
                {
                    MessageBox.Show("Can't read this file", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            View.ZoomAllView();
        }

        public void ExportModel(ModelFormat theFormat)
        {
            int aFormat = 10;
            SaveFileDialog saveDialog = new();
            string aDataDir = Environment.GetEnvironmentVariable("CSF_OCCTDataPath");
            string aFilter = "";

            switch (theFormat)
            {
                case ModelFormat.BREP:
                    saveDialog.InitialDirectory = (aDataDir + "\\occ");
                    aFormat = 0;
                    aFilter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
                    break;
                case ModelFormat.STEP:
                    saveDialog.InitialDirectory = (aDataDir + "\\step");
                    aFormat = 1;
                    aFilter = "STEP Files (*.stp *.step)|*.step; *.stp";
                    break;
                case ModelFormat.IGES:
                    saveDialog.InitialDirectory = (aDataDir + "\\iges");
                    aFormat = 2;
                    aFilter = "IGES Files (*.igs *.iges)| *.iges; *.igs";
                    break;
                case ModelFormat.VRML:
                    saveDialog.InitialDirectory = (aDataDir + "\\vrml");
                    aFormat = 3;
                    aFilter = "VRML Files (*.vrml)|*.vrml";
                    break;
                case ModelFormat.STL:
                    saveDialog.InitialDirectory = (aDataDir + "\\stl");
                    aFormat = 4;
                    aFilter = "STL Files (*.stl)|*.stl";
                    break;
                case ModelFormat.IMAGE:
                    saveDialog.InitialDirectory = (aDataDir + "\\images");
                    aFormat = 5;
                    aFilter = "Images Files (*.bmp)|*.bmp";
                    break;
                default:
                    break;
            }

            saveDialog.Filter = aFilter;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string aFileName = saveDialog.FileName;
                if (aFileName == "")
                {
                    return;
                }

                if (!View.TranslateModel(aFileName, aFormat, false))
                {
                    MessageBox.Show("Can not write this file", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public void FitAll()
        {
            View.ZoomAllView();
        }

        public void ZoomWindow()
        {
            CurrentMode = CurrentAction3d.WindowZooming;
        }

        public void DynamicZooming()
        {
            CurrentMode = CurrentAction3d.DynamicZooming;
        }

        public void DynamicPanning()
        {
            CurrentMode = CurrentAction3d.DynamicPanning;
        }

        public void DynamicRotation()
        {
            CurrentMode = CurrentAction3d.DynamicRotation;
        }

        public void GlobalPanning()
        {
            m_CurZoom = View.Scale();
            CurrentMode = CurrentAction3d.GlobalPanning;
        }

        public void AxoView()
        {
            View.AxoView();
        }

        public void FrontView()
        {
            View.FrontView();
        }

        public void BackView()
        {
            View.BackView();
        }

        public void TopView()
        {
            View.TopView();
        }

        public void BottomView()
        {
            View.BottomView();
        }

        public void LeftView()
        {
            View.LeftView();
        }

        public void RightView()
        {
            View.RightView();
        }

        public void Reset()
        {
            View.Reset();
        }

        public void HiddenOff()
        {
            View.SetDegenerateModeOff();
            DegenerateMode = false;
        }

        public void HiddenOn()
        {
            View.SetDegenerateModeOn();
            DegenerateMode = true;
        }

        public void SelectionChanged()
        {
            switch (View.DisplayMode())
            {
                case -1:
                    IsShadingEnabled = false;
                    IsWireframeEnabled = false;
                    break;
                case 0:
                    IsWireframeEnabled = false;
                    IsShadingEnabled = true;
                    IsTransparencyEnabled = false;
                    break;
                case 1:
                    IsWireframeEnabled = true;
                    IsShadingEnabled = false;
                    IsTransparencyEnabled = true;
                    break;
                case 10:
                    IsWireframeEnabled = true;
                    IsShadingEnabled = true;
                    IsTransparencyEnabled = true;
                    break;
                default:
                    break;
            }

            if (View.IsObjectSelected())
            {
                IsColorEnabled = true;
                IsMaterialEnabled = true;
                IsDeleteEnabled = true;
            }
            else
            {
                IsColorEnabled = false;
                IsMaterialEnabled = false;
                IsTransparencyEnabled = false;
                IsDeleteEnabled = false;
            }

            RaiseAvaliabiltyOfOperationsChanged();
        }

        public void Wireframe()
        {
            View.SetDisplayMode((int)DisplayMode.Wireframe);
            View.UpdateCurrentViewer();

            SelectionChanged();
            RaiseZoomingFinished();
        }

        public void Shading()
        {
            View.SetDisplayMode((int)DisplayMode.Shading);
            View.UpdateCurrentViewer();

            SelectionChanged();
            RaiseZoomingFinished();
        }

        public void ToggleRayTracing()
        {
            if (IsRayTracingEnabled)
            {
                View.DisableRayTracing();
            }
            else
            {
                View.EnableRayTracing();
            }
            IsRayTracingEnabled = !IsRayTracingEnabled;
        }

        public void ToggleAntiAliasing()
        {
            IsAntiAliasingEnabled = !IsAntiAliasingEnabled;
            View.SetAntialiasing(IsAntiAliasingEnabled);
        }

        public void Material()
        {
            //MaterialDlg aDlg = new MaterialDlg (View);
            //aDlg.ShowDialog ();
        }

        public void Transparency()
        {
            //TransparencyDialog dlg = new TransparencyDialog ();
            //dlg.View = View;
            //dlg.ShowDialog ();
        }

        public void Delete()
        {
            View.EraseObjects();
            SelectionChanged();
        }

        public IntPtr GetViewPtr()
        {
            return View.GetViewPtr();
        }

        public IntPtr GetAISContextPtr()
        {
            return View.GetAISContextPtr();
        }

        public void SetRenderRation(float ration)
        {
            View.SetRenderRation(ration);
        }

        protected void MultiDragEvent(int x, int y, int theState)
        {
            if (theState == -1) //mouse is down
            {
                m_ButtonDownX = x;
                m_ButtonDownY = y;
            }
            else if (theState == 1) //mouse is up
            {
                View.ShiftSelect(Math.Min(m_ButtonDownX, x), Math.Min(m_ButtonDownY, y),
                                 Math.Max(m_ButtonDownX, x), Math.Max(m_ButtonDownY, y));
            }
        }

        protected void DragEvent(int x, int y, int theState)
        {
            if (theState == -1) //mouse is down
            {
                m_ButtonDownX = x;
                m_ButtonDownY = y;
            }
            else if (theState == 1) //mouse is up
            {
                View.Select(Math.Min(m_ButtonDownX, x), Math.Min(m_ButtonDownY, y),
                            Math.Max(m_ButtonDownX, x), Math.Max(m_ButtonDownY, y));
            }
        }

        public void OnMouseDown(System.Windows.IInputElement sender, MouseButtonEventArgs e)
        {
            Grid? aGrid = sender as Grid;
            if (aGrid == null) return;

            Point p = new((int)e.GetPosition(aGrid).X, (int)e.GetPosition(aGrid).Y);

            // to avoid the context menu opening
            aGrid.ContextMenu.Visibility = System.Windows.Visibility.Collapsed;
            aGrid.ContextMenu.IsOpen = false;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_Xmin = p.X;
                m_Xmax = p.X;
                m_Ymin = p.Y;
                m_Ymax = p.Y;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    // start the dynamic zooming....
                    CurrentMode = CurrentAction3d.DynamicZooming;
                }
                else
                {
                    switch (CurrentMode)
                    {
                        case CurrentAction3d.Nothing:
                            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                            {
                                MultiDragEvent(m_Xmax, m_Ymax, -1);
                            }
                            else
                            {
                                DragEvent(m_Xmax, m_Ymax, -1);
                            }
                            break;
                        case CurrentAction3d.DynamicRotation:
                            if (!DegenerateMode)
                            {
                                View.SetDegenerateModeOn();
                            }
                            View.StartRotation(p.X, p.Y);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (!DegenerateMode)
                    {
                        View.SetDegenerateModeOn();
                    }
                    View.StartRotation(p.X, p.Y);
                }
                else
                {
                    // show context menu only in this case
                    aGrid.ContextMenu.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public void OnMouseUp(System.Windows.IInputElement sender, MouseButtonEventArgs e)
        {
            Point p = new((int)e.GetPosition(sender).X, (int)e.GetPosition(sender).Y);

            if (e.ChangedButton == MouseButton.Left)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    CurrentMode = CurrentAction3d.Nothing;
                    return;
                }
                switch (CurrentMode)
                {
                    case CurrentAction3d.Nothing:
                        if (p.X == m_Xmin && p.Y == m_Ymin)
                        {
                            m_Xmax = p.X;
                            m_Ymax = p.Y;
                            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                            {
                                View.ShiftSelect();
                            }
                            else
                            {
                                View.Select();
                            }
                        }
                        else
                        {
                            m_Xmax = p.X;
                            m_Ymax = p.Y;
                            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                            {
                                MultiDragEvent(m_Xmax, m_Ymax, 1);
                            }
                            else
                            {
                                DragEvent(m_Xmax, m_Ymax, 1);
                            }
                        }
                        break;
                    case CurrentAction3d.DynamicZooming:
                        CurrentMode = CurrentAction3d.Nothing;
                        break;
                    case CurrentAction3d.WindowZooming:
                        m_Xmax = p.X;
                        m_Ymax = p.Y;
                        int ValZWMin = 1;
                        if (Math.Abs(m_Xmax - m_Xmin) > ValZWMin &&
                             Math.Abs(m_Xmax - m_Ymax) > ValZWMin)
                        {
                            View.WindowFitAll(m_Xmin, m_Ymin, m_Xmax, m_Ymax);
                        }
                        RaiseZoomingFinished();
                        CurrentMode = CurrentAction3d.Nothing;
                        break;
                    case CurrentAction3d.DynamicPanning:
                        CurrentMode = CurrentAction3d.Nothing;
                        break;
                    case CurrentAction3d.GlobalPanning:
                        View.Place(p.X, p.Y, m_CurZoom);
                        CurrentMode = CurrentAction3d.Nothing;
                        break;
                    case CurrentAction3d.DynamicRotation:
                        CurrentMode = CurrentAction3d.Nothing;
                        if (!DegenerateMode)
                        {
                            View.SetDegenerateModeOff();
                        }
                        else
                        {
                            View.SetDegenerateModeOn();
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (!DegenerateMode)
                {
                    View.SetDegenerateModeOff();
                }
                else
                {
                    View.SetDegenerateModeOn();
                }
            }

            SelectionChanged();
        }

        public void OnMouseMove(System.Windows.IInputElement sender, System.Windows.Input.MouseEventArgs e)
        {
            Point p = new((int)e.GetPosition(sender).X, (int)e.GetPosition(sender).Y);

            if (e.LeftButton == MouseButtonState.Pressed) //left button is pressed
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    View.Zoom(m_Xmax, m_Ymax, p.X, p.Y);
                    m_Xmax = p.X;
                    m_Ymax = p.Y;
                }
                else
                {
                    switch (CurrentMode)
                    {
                        case CurrentAction3d.Nothing:
                            m_Xmax = p.X;
                            m_Ymax = p.Y;
                            break;
                        case CurrentAction3d.DynamicZooming:
                            View.Zoom(m_Xmax, m_Ymax, p.X, p.Y);
                            m_Xmax = p.X;
                            m_Ymax = p.Y;
                            break;
                        case CurrentAction3d.WindowZooming:
                            m_Xmax = p.X;
                            m_Ymax = p.Y;
                            break;
                        case CurrentAction3d.DynamicPanning:
                            View.Pan(p.X - m_Xmax, m_Ymax - p.Y);
                            m_Xmax = p.X;
                            m_Ymax = p.Y;
                            break;
                        case CurrentAction3d.GlobalPanning:
                            break;
                        case CurrentAction3d.DynamicRotation:
                            View.Rotation(p.X, p.Y);
                            View.RedrawView();
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (e.MiddleButton == MouseButtonState.Pressed) //middle button is pressed
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    View.Pan(p.X - m_Xmax, m_Ymax - p.Y);
                    m_Xmax = p.X;
                    m_Ymax = p.Y;
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed) //right button is pressed
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    View.Rotation(p.X, p.Y);
                }
            }
            else // no buttons are pressed
            {
                m_Xmax = p.X;
                m_Ymax = p.Y;
                View.MoveTo(p.X, p.Y);
            }
        }
    }
}
