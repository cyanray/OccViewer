using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace OccViewer.Viewer
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            TextBoxDescription.Text =
                """
                OccViewer is a simple 3D viewer based on OpenCascade. It is written in C#, WPF and C++/CLI.
                License: LGPL-2.1 license

                Opensource Components:
                1. OpenCascade (LGPL-2.1 license with additional exception).
                2. FluentIcons.WPF (MIT license).
                """;
        }

        private void GithubLink_Click(object sender, RoutedEventArgs e)
        {
            var url = GithubLink.NavigateUri.ToString();
            var sInfo = new System.Diagnostics.ProcessStartInfo(url)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }
    }
}
