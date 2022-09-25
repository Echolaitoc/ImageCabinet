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
            DirectoryNavigation.Path = startupDirectory;
            FileViewer.OnItemDoubleClick += DirectoryNavigation.OnDirectoryNavigationRequest;
        }

        private void TestThemeSwitch_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.ThemeManager.LoadTheme("UnicornCandy");
        }
    }
}
