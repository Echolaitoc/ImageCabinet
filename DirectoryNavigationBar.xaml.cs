using System;
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
        private const int PATH_HISTORY_MAX_LENGTH = 64;

        public ICommand NavigateDirectoryCommand { get; set; } = new RoutedUICommand();
        public ICommand NavigateBackCommand { get; set; } = new RoutedUICommand();
        public ICommand NavigateForwardCommand { get; set; } = new RoutedUICommand();
        public ICommand NavigateUpCommand { get; set; } = new RoutedUICommand();
        public ICommand ConfirmPathTextBoxCommand { get; set; } = new RoutedUICommand();
        public ObservableCollection<DirectoryInfo> DirectoryNavigationItems { get; private set; } = new();
        private List<string> PathHistory { get; set; } = new();
        private bool UpdateHistory { get; set; } = true;

        private int _pathHistoryPosition = -1;
        private int PathHistoryPosition
        {
            get
            {
                return _pathHistoryPosition;
            }
            set
            {
                if (PathHistory == null)
                {
                    _pathHistoryPosition = -1;
                    return;
                }
                _pathHistoryPosition = value;
                Math.Clamp(_pathHistoryPosition, 0, PathHistory.Count);
                var path = PathHistory[_pathHistoryPosition];
                if (Directory.Exists(path))
                {
                    Path = path;
                }
            }
        }

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(DirectoryNavigationBar), new FrameworkPropertyMetadata()
        {
            PropertyChangedCallback = OnPathChanged
        });
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is string strPath) || !(d is DirectoryNavigationBar bar)) return;
            if (!Directory.Exists(strPath)) return;

            bar.TextBoxVisible = false;

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

            if (bar.UpdateHistory)
            {
                if (bar.PathHistory.Count > bar.PathHistoryPosition + 1)
                {
                    bar.PathHistory.RemoveRange(bar.PathHistoryPosition + 1, bar.PathHistory.Count - (bar.PathHistoryPosition + 1));
                }
                bar.PathHistory.Add(strPath);
                ++bar.PathHistoryPosition;
                if (bar.PathHistory.Count > PATH_HISTORY_MAX_LENGTH)
                {
                    bar.PathHistory.RemoveAt(0);
                    --bar.PathHistoryPosition;
                }
            }
        }

        public static readonly DependencyProperty TextBoxVisibleProperty = DependencyProperty.Register("TextBoxVisible", typeof(bool), typeof(DirectoryNavigationBar), new FrameworkPropertyMetadata()
        {
            PropertyChangedCallback = OnTextBoxVisibleChanged
        });
        public bool TextBoxVisible
        {
            get { return (bool)GetValue(TextBoxVisibleProperty); }
            set { SetValue(TextBoxVisibleProperty, value); }
        }

        private static void OnTextBoxVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DirectoryNavigationBar bar) || (bool)e.NewValue == (bool)e.OldValue) return;

            var window = Window.GetWindow(bar);
            if ((bool)e.NewValue)
            {
                bar.PathTextBox.Text = bar.Path;
                bar.PathTextBox.Visibility = Visibility.Visible;
                bar.PathTextBox.Focus();
                bar.PathTextBox.SelectAll();
                bar.PathTextBox.LostFocus -= bar.PathTextBox_LostFocus;
                bar.PathTextBox.LostFocus += bar.PathTextBox_LostFocus;

                if (window != null)
                {
                    window.MouseDown -= bar.Window_MouseDown;
                    window.MouseDown += bar.Window_MouseDown;
                }

                if (Application.Current != null)
                {
                    Application.Current.Deactivated -= bar.App_Deactivated;
                    Application.Current.Deactivated += bar.App_Deactivated;
                }
            }
            else
            {
                if (window != null)
                {
                    window.MouseDown -= bar.Window_MouseDown;
                }

                if (Application.Current != null)
                {
                    Application.Current.Deactivated -= bar.App_Deactivated;
                }

                if (Directory.Exists(bar.PathTextBox.Text))
                {
                    bar.Path = bar.PathTextBox.Text;
                }

                Keyboard.ClearFocus();
                bar.PathTextBox.Visibility = Visibility.Collapsed;
            }
        }

        public DirectoryNavigationBar()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(NavigateDirectoryCommand, Navigate));
            CommandBindings.Add(new CommandBinding(NavigateBackCommand, NavigateBack, CanNavigateBack));
            CommandBindings.Add(new CommandBinding(NavigateForwardCommand, NavigateForward, CanNavigateForward));
            CommandBindings.Add(new CommandBinding(NavigateUpCommand, NavigateUp, CanNavigateUp));
            CommandBindings.Add(new CommandBinding(ConfirmPathTextBoxCommand, ConfirmPathTextBox));
        }

        private void Navigate(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is DirectoryInfo dirInfo)) return;
            Path = dirInfo.FullName;
        }

        public void NavigateBack()
        {
            if (CanNavigateBack())
            {
                DoNavigateBack();
            }
        }

        private void NavigateBack(object sender, ExecutedRoutedEventArgs e)
        {
            DoNavigateBack();
        }

        private void DoNavigateBack()
        {
            UpdateHistory = false;
            --PathHistoryPosition;
            UpdateHistory = true;
        }

        private void CanNavigateBack(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanNavigateBack();
        }

        private bool CanNavigateBack()
        {
            if (PathHistory != null && PathHistory.Count > 0)
            {
                return PathHistoryPosition > 0;
            }
            return false;
        }

        public void NavigateForward()
        {
            if (CanNavigateForward())
            {
                DoNavigateForward();
            }
        }

        private void NavigateForward(object sender, ExecutedRoutedEventArgs e)
        {
            DoNavigateForward();
        }

        private void DoNavigateForward()
        {
            UpdateHistory = false;
            ++PathHistoryPosition;
            UpdateHistory = true;
        }

        private void CanNavigateForward(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanNavigateForward();
        }

        private bool CanNavigateForward()
        {
            if (PathHistory != null && PathHistory.Count > 0)
            {
                return PathHistoryPosition < PathHistory.Count - 1;
            }
            return false;
        }

        private void NavigateUp(object sender, ExecutedRoutedEventArgs e)
        {
            if (Directory.Exists(Path))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path);
                if (dirInfo.Parent != null)
                {
                    Path = dirInfo.Parent.FullName;
                }
            }
        }

        private void CanNavigateUp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (Directory.Exists(Path))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path);
                e.CanExecute = dirInfo.Parent != null;
            }
        }

        private void ConfirmPathTextBox(object sender, ExecutedRoutedEventArgs e)
        {
            TextBoxVisible = false;
        }

        private void GridMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBoxVisible = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBoxVisible = false;
        }

        private void App_Deactivated(object? sender, EventArgs e)
        {
            TextBoxVisible = false;
        }

        private void PathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxVisible = false;
        }

        public void OnDirectoryNavigationRequest(object? sender, EventArgs e)
        {
            if (!(sender is FileSystemItem fileSystemItem) || !fileSystemItem.IsDirectory) return;
            Path = fileSystemItem.Path;
        }
    }
}
