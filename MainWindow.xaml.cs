using System;
using System.Windows;

namespace ImageCabinet
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DirectoryNavigation.Path = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%");
        }
    }
}
