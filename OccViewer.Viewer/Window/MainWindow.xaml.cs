using Microsoft.Win32;
using OccViewer.Viewer.Common;
using OccViewer.Viewer.Shortcut;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace OccViewer.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public OCCViewer ActiveViewer { get; private set; }

        private D3DViewer D3DViewer { get; set; }

        protected void RaisePropertyChanged(string thePropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(thePropertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            InitViewer();
        }

        [MemberNotNull(nameof(D3DViewer), nameof(ActiveViewer))]
        private void InitViewer()
        {
            D3DViewer = new D3DViewer();
            ImageBrush anImage = new(D3DViewer.Image);
            ViewerGrid.Background = anImage;
            ActiveViewer = D3DViewer.Viewer;
            ActiveViewer.ActionShortcuts = new IntuitiveActionShortcuts();
        }

        private void ViewerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            D3DViewer.Resize(Convert.ToInt32(e.NewSize.Width * dpi.DpiScaleX),
                              Convert.ToInt32(e.NewSize.Height * dpi.DpiScaleY));
        }

        void ViewerGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ActiveViewer.HandleMouseUp(ViewerGrid, e);
        }

        void ViewerGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ActiveViewer.HandleMouseDown(ViewerGrid, e);
        }

        void ViewerGrid_MouseMove(object sender, MouseEventArgs e)
        {
            ActiveViewer.HandleMouseMove(ViewerGrid, e);
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = FileFilterConstants.ImportFilterString
            };
            var result = dialog.ShowDialog();
            if (result != null && (bool)result)
            {
                string filename = dialog.FileName;
                if (filename == "") return;
                ModelFormat format;
                if (dialog.FilterIndex != FileFilterConstants.ImportFilterArray.Length)
                {
                    format = FileFilterConstants.ImportFormatArray[dialog.FilterIndex - 1];
                }
                else
                {
                    string ext = System.IO.Path.GetExtension(filename);
                    format = FileFilterConstants.GetModelFormatByExtension(ext);
                }
                if (format == ModelFormat.Unknown)
                {
                    MessageBox.Show("Unknown model format", "Error!", MessageBoxButton.OK);
                    return;
                }
                if (!ActiveViewer.ImportModel(format, filename))
                {
                    MessageBox.Show("Can't read this file", "Error!", MessageBoxButton.OK);
                    return;
                }
                ActiveViewer.View.ZoomAllView();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = FileFilterConstants.ExportFilterString
            };
            var result = dialog.ShowDialog();
            if (result != null && (bool)result)
            {
                string filename = dialog.FileName;
                if (filename == "") return;
                ModelFormat format = FileFilterConstants.ExportFormatArray[dialog.FilterIndex - 1];
                if (!ActiveViewer.ExportModel(format, filename))
                {
                    MessageBox.Show("Can't read this file", "Error!", MessageBoxButton.OK);
                    return;
                }
                ActiveViewer.View.ZoomAllView();
            }
        }

        private void BtnFitAll_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.FitAll();
        }

        private void BtnZoomWindow_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.ZoomWindow();
        }

        private void BtnDynamicZoom_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.DynamicZooming();
        }

        private void BtnPan_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.DynamicPanning();
        }

        private void BtnRotate_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.DynamicRotation();
        }

        private void BtnFront_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.FrontView();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.BackView();
        }

        private void BtnTop_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.TopView();
        }

        private void BtnBottom_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.BottomView();
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.LeftView();
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.RightView();
        }

        private void BtnAxoView_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.AxoView();
        }

        private void BtnWireframe_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.Wireframe();
        }

        private void BtnShade_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.Shading();
        }

        private void BtnSetColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog ColDlg = new()
            {
                Color = ActiveViewer.CurrentObjectColor
            };
            if (ColDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ActiveViewer.CurrentObjectColor = ColDlg.Color;
            }
        }

        private void MenuChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog ColDlg = new()
            {
                Color = ActiveViewer.BackgroundColor
            };
            if (ColDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ActiveViewer.BackgroundColor = ColDlg.Color;
            }
        }

        private void BtnSetMaterial_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSetTransparency_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRayTracing_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.ToggleRayTracing();
        }

        private void BtnAntiAliasing_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.ToggleAntiAliasing();
        }

        private void ToggleTriedron_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer.DisplayTriedron(!ActiveViewer!.IsTriedronEnabled);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UseDefaultShortcut_Click(object sender, RoutedEventArgs e)
        {
            UseDefaultShortcut.IsChecked = true;
            UseIntuitiveShortcut.IsChecked = false;
            ActiveViewer.ActionShortcuts = new DefaultActionShortcuts();
        }

        private void UseIntuitiveShortcut_Click(object sender, RoutedEventArgs e)
        {
            UseDefaultShortcut.IsChecked = false;
            UseIntuitiveShortcut.IsChecked = true;
            ActiveViewer.ActionShortcuts = new IntuitiveActionShortcuts();
        }
    }
}
