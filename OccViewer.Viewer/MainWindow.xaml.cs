using IE_WPF_D3D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace OccViewer.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public OCCViewer? ActiveViewer { get; private set; }

        private D3DViewer? D3DViewer { get; set; }

        private Grid? ViewerGrid { get; set; }

        protected void RaisePropertyChanged(string thePropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(thePropertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            InitViewer();
        }

        private void InitViewer()
        {
            D3DViewer = new D3DViewer();
            Grid g = new Grid();
            g.Background = new SolidColorBrush(Colors.Black);
            ImageBrush anImage = new ImageBrush(D3DViewer.Image);
            g.Background = anImage;
            g.MouseMove += new MouseEventHandler(g_MouseMove);
            g.MouseDown += new MouseButtonEventHandler(g_MouseDown);
            g.MouseUp += new MouseButtonEventHandler(g_MouseUp);
            g.SizeChanged += new SizeChangedEventHandler(g_SizeChanged);
            g.HorizontalAlignment = HorizontalAlignment.Stretch;
            ViewerRoot.Children.Add(g);
            ViewerGrid = g;
            ActiveViewer = D3DViewer.Viewer;
        }

        private void g_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            D3DViewer?.Resize(Convert.ToInt32(e.NewSize.Width),
                              Convert.ToInt32(e.NewSize.Height));
        }

        void g_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewerGrid != null)
            {
                ActiveViewer?.OnMouseUp(ViewerGrid, e);
            }
        }

        void g_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewerGrid != null)
            {
                ActiveViewer?.OnMouseDown(ViewerGrid, e);
            }
        }

        void g_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewerGrid != null)
            {
                ActiveViewer?.OnMouseMove(ViewerGrid, e);
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            ActiveViewer?.ImportModel(ModelFormat.IGES);
        }
    }
}
