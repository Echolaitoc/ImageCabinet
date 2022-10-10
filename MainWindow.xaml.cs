using System;
using System.Windows;

namespace ImageCabinet
{
    public partial class MainWindow : Window
    {
        private static string DEFAULT_DIRECTORY = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%");

        public MainWindow() : this(DEFAULT_DIRECTORY) {}

        public MainWindow(string startupDirectory)
        {
            InitializeComponent();
            if (!System.IO.Directory.Exists(startupDirectory))
            {
                startupDirectory = DEFAULT_DIRECTORY;
            }
            DirectoryNavigation.Path = Environment.ExpandEnvironmentVariables(startupDirectory);
            FileViewer.OnItemDoubleClick += DirectoryNavigation.OnDirectoryNavigationRequest;
        }

        private void MainWindow_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.XButton1 && DirectoryNavigation != null)
            {
                DirectoryNavigation.NavigateBack();
            }
            else if (e.ChangedButton == System.Windows.Input.MouseButton.XButton2 && DirectoryNavigation != null)
            {
                DirectoryNavigation.NavigateForward();
            }
        }
    }
}
