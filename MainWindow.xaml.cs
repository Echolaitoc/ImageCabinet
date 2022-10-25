using ImageCabinet.Settings;
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

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Config.Current.TrySetValue(nameof(Config.Current.WindowWidth), ActualWidth);
            Config.Current.TrySetValue(nameof(Config.Current.WindowHeight), ActualHeight);
            Config.Current.TrySetValue(nameof(Config.Current.WindowPositionX), Left);
            Config.Current.TrySetValue(nameof(Config.Current.WindowPositionY), Top);
            ConfigXml.WriteSettingsToXml();
        }
    }
}
