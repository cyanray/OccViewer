using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using OccViewer.Viewer.Shortcut;
using System.Diagnostics.CodeAnalysis;

namespace OccViewer.Viewer
{

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
        #region PropertiesAndMembers

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

        public IActionShortcuts ActionShortcuts
        {
            get
            {
                return m_ActionShortcuts!;
            }

            [MemberNotNull(nameof(ShortcutToActionMap))]
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                m_ActionShortcuts = value;
                ShortcutToActionMap = new Dictionary<CombineShortcut, ActionStatus>
                {
                    { value.RectangleSelectionShortcut, ActionStatus.RectangleSelection },
                    { value.RectangleSelectionXorShortcut, ActionStatus.RectangleSelectionXor },
                    { value.DynamicZoomingShortcut, ActionStatus.DynamicZooming },
                    { value.WindowZoomingShortcut, ActionStatus.WindowZooming },
                    { value.DynamicPanningShortcut, ActionStatus.DynamicPanning },
                    { value.DynamicRotationShortcut, ActionStatus.DynamicRotation },
                    { value.PickSelectionShortcut, ActionStatus.PickSelection },
                    { value.PickSelectionXorShortcut, ActionStatus.PickSelectionXor },
                    { value.PopupMenuShortcut, ActionStatus.PopupMenu }
                };
            }
        }

        public OCCTProxyD3D View { get; private set; }

        public bool DegenerateMode { get; private set; }

        public bool IsWireframeEnabled { get; private set; }

        public bool IsShadingEnabled { get; private set; }

        public bool IsRayTracingEnabled { get; private set; }

        public bool IsAntiAliasingEnabled { get; private set; }

        public bool IsTransparencyEnabled { get; private set; }

        public bool IsColorEnabled { get; private set; }

        public bool IsMaterialEnabled { get; private set; }

        public bool IsDeleteEnabled { get; private set; }

        public bool IsTriedronEnabled { get; private set; } = true;

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

        private Dictionary<CombineShortcut, ActionStatus> ShortcutToActionMap { get; set; }

        private int m_Xmin;
        private int m_Ymin;
        private int m_Xmax;
        private int m_Ymax;

        private ActionStatus m_CurrentAction = ActionStatus.None;

        private ActionStatus m_CurrentActionOverride = ActionStatus.None;

        private PressedKey m_CurrentPressedKey = PressedKey.None;

        private MouseButtonTrigger m_CurrentMouseTrigger = MouseButtonTrigger.None;

        private IActionShortcuts? m_ActionShortcuts;

        #endregion

        #region PublicMethods

        public OCCViewer()
        {
            View = new OCCTProxyD3D();
            View.InitOCCTProxy();
            DegenerateMode = false;
            ActionShortcuts = new DefaultActionShortcuts();
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
            m_CurrentActionOverride = ActionStatus.WindowZooming;
        }

        public void DynamicZooming()
        {
            m_CurrentActionOverride = ActionStatus.DynamicZooming;
        }

        public void DynamicPanning()
        {
            m_CurrentActionOverride = ActionStatus.DynamicPanning;
        }

        public void DynamicRotation()
        {
            m_CurrentActionOverride = ActionStatus.DynamicRotation;
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
            DegenerateMode = false;
            View.SetDegenerateMode(DegenerateMode);
        }

        public void HiddenOn()
        {
            DegenerateMode = true;
            View.SetDegenerateMode(DegenerateMode);
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

        public void SetMsaaSamples(int samples)
        {
            View.SetMsaaSamples(samples);
        }

        public void DisplayTriedron(bool show)
        {
            IsTriedronEnabled = show;
            View.DisplayTriedron(show);
        }

        #endregion

        #region PrivateMembers

        private void SelectionChanged()
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

        private static (double DpiScaleX, double DpiScaleY) GetDpiScale(object element)
        {
            System.Windows.DpiScale dpi = System.Windows.Media.VisualTreeHelper.GetDpi((System.Windows.Media.Visual)element);
            return (dpi.DpiScaleX, dpi.DpiScaleY);
        }

        private void UpdateCurrentPressedKey()
        {
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            m_CurrentPressedKey = ctrl ? PressedKey.Ctrl : PressedKey.None;
            m_CurrentPressedKey |= shift ? PressedKey.Shift : PressedKey.None;
            m_CurrentPressedKey |= alt ? PressedKey.Alt : PressedKey.None;
        }

        private void UpdatePressedMouseButton(MouseButton button)
        {
            m_CurrentMouseTrigger = button switch
            {
                MouseButton.Left => MouseButtonTrigger.LeftPressed,
                MouseButton.Middle => MouseButtonTrigger.MiddlePressed,
                MouseButton.Right => MouseButtonTrigger.RightPressed,
                _ => MouseButtonTrigger.None,
            };
        }

        private void UpdateClickedMouseButton(MouseButton button)
        {
            if (m_Xmin != m_Xmax || m_Ymin != m_Ymax) return;
            m_CurrentMouseTrigger = button switch
            {
                MouseButton.Left => MouseButtonTrigger.LeftClicked,
                MouseButton.Middle => MouseButtonTrigger.MiddleClicked,
                MouseButton.Right => MouseButtonTrigger.RightClicked,
                _ => MouseButtonTrigger.None,
            };
        }


        private void UpdateCurrentAction()
        {
            m_CurrentAction = m_CurrentActionOverride;
            if (m_CurrentAction != ActionStatus.None) return;
            CombineShortcut shortcut = new(m_CurrentPressedKey, m_CurrentMouseTrigger);
            if (ShortcutToActionMap.TryGetValue(shortcut, out ActionStatus action))
            {
                m_CurrentAction = action;
            }
            Debug.WriteLine($"m_CurrentPressedKey, m_CurrentPressedMouseButton: {m_CurrentPressedKey}, {m_CurrentMouseTrigger}");
            Debug.WriteLine($"m_CurrentAction: {m_CurrentAction}");
        }

        public void HandleMouseDown(System.Windows.IInputElement sender, MouseButtonEventArgs e)
        {
            if (sender is not Grid aGrid) return;
            aGrid.ContextMenu.Visibility = System.Windows.Visibility.Collapsed;
            aGrid.ContextMenu.IsOpen = false;
            var (DpiScaleX, DpiScaleY) = GetDpiScale(sender);
            Point p = new((int)(e.GetPosition(sender).X * DpiScaleX), (int)(e.GetPosition(sender).Y * DpiScaleY));
            UpdateCurrentPressedKey();
            UpdatePressedMouseButton(e.ChangedButton);
            UpdateCurrentAction();
            m_Xmin = m_Xmax = p.X; m_Ymin = m_Ymax = p.Y;
            switch (m_CurrentAction)
            {
                case ActionStatus.WindowZooming:
                case ActionStatus.RectangleSelection:
                case ActionStatus.RectangleSelectionXor:
                    View.BeginRubberBand();
                    break;
                case ActionStatus.DynamicRotation:
                    View.StartRotation(p.X, p.Y);
                    break;
                default:
                    break;
            }

        }

        public void HandleMouseUp(System.Windows.IInputElement sender, MouseButtonEventArgs e)
        {
            var (DpiScaleX, DpiScaleY) = GetDpiScale(sender);
            Point p = new((int)(e.GetPosition(sender).X * DpiScaleX), (int)(e.GetPosition(sender).Y * DpiScaleY));
            int Height = (int)(((System.Windows.FrameworkElement)sender).ActualHeight * DpiScaleY);
            UpdateCurrentPressedKey();
            UpdateClickedMouseButton(e.ChangedButton);
            UpdateCurrentAction();

            SelectionScheme scheme = m_CurrentAction switch
            {
                ActionStatus.PickSelectionXor => SelectionScheme.Xor,
                ActionStatus.RectangleSelectionXor => SelectionScheme.Xor,
                _ => SelectionScheme.Replace,
            };

            switch (m_CurrentAction)
            {
                case ActionStatus.PickSelection:
                case ActionStatus.PickSelectionXor:
                    View.PickSelection(scheme);
                    SelectionChanged();
                    break;
                case ActionStatus.RectangleSelection:
                case ActionStatus.RectangleSelectionXor:
                    View.EndRubberBand();
                    View.RectangleSelection(m_Xmin, Height - m_Ymin, m_Xmax, Height - m_Ymax, scheme);
                    SelectionChanged();
                    break;
                case ActionStatus.WindowZooming:
                    int ValZWMin = 1;
                    View.EndRubberBand();
                    if (Math.Abs(m_Xmax - m_Xmin) > ValZWMin && Math.Abs(m_Xmax - m_Ymax) > ValZWMin)
                    {
                        View.WindowFitAll(m_Xmin, m_Ymin, m_Xmax, m_Ymax);
                    }
                    RaiseZoomingFinished();
                    break;
                case ActionStatus.DynamicZooming:
                    break;
                case ActionStatus.DynamicPanning:
                    break;
                case ActionStatus.DynamicRotation:
                    break;
                case ActionStatus.PopupMenu:
                    if (sender is Grid aGrid)
                    {
                        aGrid.ContextMenu.Visibility = System.Windows.Visibility.Visible;
                        aGrid.ContextMenu.IsOpen = true;
                    }
                    break;
                default:
                    break;
            }
            m_Xmax = p.X;
            m_Ymax = p.Y;
            m_CurrentAction = ActionStatus.None;
            m_CurrentActionOverride = ActionStatus.None;
        }

        public void HandleMouseMove(System.Windows.IInputElement sender, System.Windows.Input.MouseEventArgs e)
        {
            var (DpiScaleX, DpiScaleY) = GetDpiScale(sender);
            Point p = new((int)(e.GetPosition(sender).X * DpiScaleX), (int)(e.GetPosition(sender).Y * DpiScaleY));

            switch (m_CurrentAction)
            {
                case ActionStatus.None:
                    m_Xmax = p.X;
                    m_Ymax = p.Y;
                    View.MoveTo(p.X, p.Y);
                    break;
                case ActionStatus.RectangleSelection:
                case ActionStatus.RectangleSelectionXor:
                case ActionStatus.WindowZooming:
                    m_Xmax = p.X;
                    m_Ymax = p.Y;
                    int Height = (int)(((System.Windows.FrameworkElement)sender).ActualHeight * DpiScaleY);
                    View.UpdateRubberBand(m_Xmin, Height - m_Ymin, m_Xmax, Height - m_Ymax);
                    Debug.WriteLine($"RubberBand:{m_Xmin}, {Height - m_Ymin}, {m_Xmax}, {Height - m_Ymax}");
                    break;
                case ActionStatus.DynamicZooming:
                    View.Zoom(m_Xmax, m_Ymax, p.X, p.Y);
                    m_Xmax = p.X;
                    m_Ymax = p.Y;
                    break;
                case ActionStatus.DynamicPanning:
                    View.Pan(p.X - m_Xmax, m_Ymax - p.Y);
                    m_Xmax = p.X;
                    m_Ymax = p.Y;
                    break;
                case ActionStatus.DynamicRotation:
                    View.Rotation(p.X, p.Y);
                    View.RedrawView(); 
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
