using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageCabinet
{
    public partial class DirectoryNavigationBar : UserControl
    {
        public ICommand NavigateDirectoryCommand { get; set; } = new RoutedUICommand();

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(DirectoryNavigationBar), new FrameworkPropertyMetadata()
        {
            PropertyChangedCallback = OnPathChanged
        });
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public ObservableCollection<DirectoryInfo> DirectoryNavigationItems { get; private set; } = new ObservableCollection<DirectoryInfo>();

        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is string strPath) || !(d is DirectoryNavigationBar bar)) return;
            if (!Directory.Exists(strPath)) return;

            var dirInfos = new List<DirectoryInfo>();
            var dirInfo = new DirectoryInfo(strPath);
            while (dirInfo != null && dirInfo.Exists)
            {
                dirInfos.Add(dirInfo);
                dirInfo = dirInfo.Parent;
            }
            dirInfos.Reverse();
            bar.DirectoryNavigationItems.Clear();
            dirInfos.ForEach(dirInfo => bar.DirectoryNavigationItems.Add(dirInfo));
        }

        public DirectoryNavigationBar()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(NavigateDirectoryCommand, Navigate));
        }

        private void Navigate(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is DirectoryInfo dirInfo)) return;
            Path = dirInfo.FullName;
        }
    }
}
