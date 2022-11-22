using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace ImageCabinet
{
    public partial class FileView : UserControl
    {
        private struct LoadFilesBackgroundWorkerArguments
        {
            public bool IncludeFilesInSubfolder { get; set; }
        }

        private static List<string> SUPPORTED_FILE_EXTENSIONS = new()
        {
            ".jpg",
            ".png",
        };

        private const int PROGRESS_COMPLETED_DISPLAY_TIME_MILLISECONDS = 2000;

        public event EventHandler? OnItemDoubleClick;
        public ICommand ItemDoubleClickCommand { get; set; } = new RoutedUICommand();
        private ThumbnailCache Thumbnails { get; set; } = new();
        private BackgroundWorker LoadFilesBackgroundWorker { get; } = new();
        private FileSystemWatcher FileSystemWatcher { get; set; } = new();

        public ObservableCollection<FileSystemItem> FileSystemItems { get; set; } = new();
        private Queue<string> PendingPaths { get; set; } = new();
        private Queue<Tuple<IEnumerable<string>, bool>> PendingDirectories { get; set; } = new();
        private Queue<IEnumerable<string>> PendingFiles { get; set; } = new();
        private bool CurrentlyGeneratingPendingItems { get; set; } = false;

        private Point RelativeScrollPosition { get; set; } = new(0.0, 0.0);

        private static readonly DependencyPropertyKey ProgressPropertyKey = DependencyProperty.RegisterReadOnly("Progress", typeof(double), typeof(FileView), new PropertyMetadata(0.0));
        public static readonly DependencyProperty ProgressProperty = ProgressPropertyKey.DependencyProperty;
        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            private set { SetValue(ProgressPropertyKey, value); }
        }

        private int _progressCurrentItemCounter = 0;
        public int ProgressCurrentItemCounter
        {
            get { return _progressCurrentItemCounter; }
            set
            {
                _progressCurrentItemCounter = value;
                UpdateProgress();
            }
        }

        private int _progressTotalItemCounter = 0;
        public int ProgressTotalItemCounter
        {
            get { return _progressTotalItemCounter; }
            set
            {
                _progressTotalItemCounter = value;
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            Dispatcher.Invoke(() => Progress = ProgressTotalItemCounter > 0 ? (double)ProgressCurrentItemCounter / (double)ProgressTotalItemCounter : 0);
        }

        private static readonly DependencyPropertyKey ProgressStatePropertyKey = DependencyProperty.RegisterReadOnly("ProgressState", typeof(TaskbarItemProgressState), typeof(FileView), new PropertyMetadata(TaskbarItemProgressState.None));
        public static readonly DependencyProperty ProgressStateProperty = ProgressStatePropertyKey.DependencyProperty;
        public TaskbarItemProgressState ProgressState
        {
            get { return (TaskbarItemProgressState)GetValue(ProgressStateProperty); }
            private set { SetValue(ProgressStatePropertyKey, value); }
        }

        private Timer ProgressCompletedTimer { get; set; } = new Timer(PROGRESS_COMPLETED_DISPLAY_TIME_MILLISECONDS);
        private DateTime LoadingStartedTime { get; set; } = DateTime.Now;

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(FileView), new FrameworkPropertyMetadata()
        {
            PropertyChangedCallback = OnPathChanged
        });
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
        private string PathForBackgroundWorker { get; set; }

        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is string strPath) || !Directory.Exists(strPath) || !(d is FileView fileView)) return;
            fileView.ResetFileSystemItems();
            fileView.ScrollToTop();
            fileView.ProgressState = TaskbarItemProgressState.Indeterminate;
            fileView.EnqueuePendingPath(strPath);
            fileView.FileSystemWatcher.Path = strPath;
            fileView.FileSystemWatcher.EnableRaisingEvents = true;
        }

        private void ResetFileSystemItems()
        {
            Thumbnails.ClearCache();
            CancelLoadFilesBackgroundWorker();
            FileSystemItems.Clear();
            PendingPaths.Clear();
            PendingDirectories.Clear();
            PendingFiles.Clear();
            ProgressState = TaskbarItemProgressState.None;
            ProgressTotalItemCounter = 0;
            ProgressCurrentItemCounter = 0;
            LoadingStartedTime = DateTime.Now;
        }

        private void LoadFilesBackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (!(sender is BackgroundWorker backgroundWorker)) return;
            try
            {
                while (PendingPaths.Count > 0 && !LoadFilesBackgroundWorker.CancellationPending)
                {
                    var path = PendingPaths.Dequeue();
                    if (!Directory.Exists(path)) return;
                    if (IncludeFilesInSubfolderForBackgroundWorker)
                    {
                        AddSubfoldersToPendingPaths(path);
                    }
                    PendingDirectories.Enqueue(new(Directory.EnumerateDirectories(path), path == PathForBackgroundWorker ? false : true));
                    PendingFiles.Enqueue(Directory.EnumerateFiles(path));
                    Dispatcher.Invoke(() => LoadFilesBackgroundWorker_ProgressChanged(null, new(0, null)));
                    backgroundWorker.ReportProgress(0);
                }
            }
            catch
            {
                UIHelper.UIHelper.HandleGenericException();
            }
        }

        private void LoadFilesBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            ProgressCurrentItemCounter = ProgressTotalItemCounter - PendingPaths.Count;
            if (LoadFilesBackgroundWorker.CancellationPending || CurrentlyGeneratingPendingItems) return;

            try
            {
                CurrentlyGeneratingPendingItems = true;
                while (PendingDirectories.Count > 0)
                {
                    var dirs = PendingDirectories.Dequeue();
                    foreach (var dir in dirs.Item1)
                    {
                        FileSystemItems.Add(new FileSystemItem(new DirectoryInfo(dir)) { IsSubfolder = dirs.Item2 });
                    }
                }
                while (PendingFiles.Count > 0)
                {
                    var files = PendingFiles.Dequeue();
                    foreach (var file in files)
                    {
                        if (!File.Exists(file))
                        {
                            continue;
                        }
                        var fileInfo = new FileInfo(file);
                        if (SUPPORTED_FILE_EXTENSIONS.Any(ext => file.EndsWith(ext)))
                        {
                            FileSystemItems.Add(new ImageItem(fileInfo));
                        }
                        else
                        {
                            FileSystemItems.Add(new FileSystemItem(fileInfo));
                        }
                    }
                }
                CurrentlyGeneratingPendingItems = false;
            }
            catch
            {
                UIHelper.UIHelper.HandleGenericException();
            }
        }

        private void LoadFilesBackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (PendingPaths.Count > 0)
            {
                IncludeFilesInSubfolderForBackgroundWorker = IncludeFilesInSubfolder;
                PathForBackgroundWorker = Path;
                LoadFilesBackgroundWorker.RunWorkerAsync();
            }
            else
            {
                ProgressCurrentItemCounter = ProgressTotalItemCounter;
                ProgressCompletedTimer.Stop();
                var progressCompleteShowDuration = Math.Min(PROGRESS_COMPLETED_DISPLAY_TIME_MILLISECONDS, (DateTime.Now - LoadingStartedTime).TotalMilliseconds);
                if (progressCompleteShowDuration > 0)
                {
                    ProgressCompletedTimer.Interval = progressCompleteShowDuration;
                    ProgressCompletedTimer.Start();
                }
                else
                {
                    HideProgress();
                }
            }
        }

        private void ProgressCompletedTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ProgressCompletedTimer.Stop();
            HideProgress();
        }

        private void HideProgress()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressState = TaskbarItemProgressState.None;
                ProgressCurrentItemCounter = 0;
                ProgressTotalItemCounter = 0;
            });
        }

        private void ScrollToTop()
        {
            if (FileViewListBox == null) return;

            var scrollViewer = UIHelper.UIHelper.GetVisualChild<ScrollViewer>(FileViewListBox);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToTop();
            }
        }

        public static readonly DependencyProperty IncludeFilesInSubfolderProperty = DependencyProperty.Register("IncludeFilesInSubfolder", typeof(bool), typeof(FileView), new FrameworkPropertyMetadata(false)
        {
            PropertyChangedCallback = OnIncludeFilesInSubfolderChanged
        });
        public bool IncludeFilesInSubfolder
        {
            get { return (bool)GetValue(IncludeFilesInSubfolderProperty); }
            set { SetValue(IncludeFilesInSubfolderProperty, value); }
        }
        private bool IncludeFilesInSubfolderForBackgroundWorker { get; set; }

        private static void OnIncludeFilesInSubfolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FileView fileView)) return;
            if (e.NewValue is bool includeFilesInSubfolder)
            {
                if (includeFilesInSubfolder)
                {
                    fileView.CancelLoadFilesBackgroundWorker();
                    fileView.AddSubfoldersToPendingPaths(fileView.Path);
                }
                else
                {
                    fileView.ResetFileSystemItems();
                    fileView.EnqueuePendingPath(fileView.Path);
                }
                fileView.FileSystemWatcher.IncludeSubdirectories = includeFilesInSubfolder;
            }
        }

        private void AddSubfoldersToPendingPaths(string path)
        {
            if (!Directory.Exists(path)) return;
            var dirs = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
            foreach (var dir in dirs)
            {
                EnqueuePendingPath(dir);
            }
        }

        private void EnqueuePendingPath(string path, bool updateBackgroundWorkerProperties = true)
        {
            if (PendingPaths.Contains(path)) return;
            PendingPaths.Enqueue(path);
            ++ProgressTotalItemCounter;
            Dispatcher.Invoke(() => ProgressState = TaskbarItemProgressState.Normal);
            if (!LoadFilesBackgroundWorker.IsBusy)
            {
                if (updateBackgroundWorkerProperties)
                {
                    IncludeFilesInSubfolderForBackgroundWorker = IncludeFilesInSubfolder;
                    PathForBackgroundWorker = Path;
                }
                LoadFilesBackgroundWorker.RunWorkerAsync();
            }
        }

        private void CancelLoadFilesBackgroundWorker()
        {
            if (LoadFilesBackgroundWorker.IsBusy)
            {
                LoadFilesBackgroundWorker.CancelAsync();
            }
            Dispatcher.Invoke(() => ProgressState = TaskbarItemProgressState.None);
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    {
                        if (Directory.Exists(e.FullPath))
                        {
                            var isSubfolder = new DirectoryInfo(e.FullPath).Parent?.FullName != PathForBackgroundWorker;
                            PendingDirectories.Enqueue(new Tuple<IEnumerable<string>, bool>(new List<string>() { e.FullPath }, isSubfolder));
                        }
                        else if (File.Exists(e.FullPath))
                        {
                            PendingFiles.Enqueue(new List<string>() { e.FullPath });
                        }
                        Dispatcher.Invoke(() => LoadFilesBackgroundWorker_ProgressChanged(null, new(0, null)));
                        break;
                    }
                case WatcherChangeTypes.Deleted:
                    {
                        if (FileSystemItems.FirstOrDefault(fileItem => fileItem.Path == e.FullPath) is FileSystemItem fileSystemItem)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (fileSystemItem.IsDirectory)
                                {
                                    var itemsToRemove = FileSystemItems.Where(fileItem => fileItem.Path.StartsWith(fileSystemItem.Path + System.IO.Path.PathSeparator)).ToList();
                                    foreach (var itemToRemove in itemsToRemove)
                                    {
                                        FileSystemItems.Remove(itemToRemove);
                                    }
                                }
                                FileSystemItems.Remove(fileSystemItem);
                            });
                        }
                        break;
                    }
                case WatcherChangeTypes.Changed:
                    {
                        if (FileSystemItems.FirstOrDefault(fileItem => fileItem.Path == e.FullPath) is ImageItem imageItem)
                        {
                            Thumbnails.ReloadImage(imageItem);
                        }
                        break;
                    }
                case WatcherChangeTypes.Renamed:
                    break;
                default:
                    break;
            }
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (FileSystemItems.FirstOrDefault(fileItem => fileItem.Path == e.OldFullPath) is FileSystemItem fileSystemItem)
            {
                Dispatcher.Invoke(() =>
                {
                    if (Directory.Exists(e.FullPath))
                    {
                        fileSystemItem.UpdateFileSystemInfo(new DirectoryInfo(e.FullPath));
                    }
                    else if (File.Exists(e.FullPath))
                    {
                        fileSystemItem.UpdateFileSystemInfo(new FileInfo(e.FullPath));
                    }
                });
            }
        }

        public FileView()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(ItemDoubleClickCommand, HandleItemDoubleClick));

            PathForBackgroundWorker = string.Empty;

            LoadFilesBackgroundWorker.WorkerSupportsCancellation = true;
            LoadFilesBackgroundWorker.WorkerReportsProgress = true;
            LoadFilesBackgroundWorker.DoWork += LoadFilesBackgroundWorker_DoWork;
            LoadFilesBackgroundWorker.ProgressChanged += LoadFilesBackgroundWorker_ProgressChanged;
            LoadFilesBackgroundWorker.RunWorkerCompleted += LoadFilesBackgroundWorker_RunWorkerCompleted;

            ProgressCompletedTimer.Elapsed += ProgressCompletedTimer_Elapsed;

            FileSystemWatcher.Changed += FileSystemWatcher_Changed;
            FileSystemWatcher.Created += FileSystemWatcher_Changed;
            FileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            FileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
            FileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.Security;
        }

        private void HandleItemDoubleClick(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is FileSystemItem fileSystemItem)) return;

            if (fileSystemItem.IsDirectory && OnItemDoubleClick != null)
            {
                OnItemDoubleClick.Invoke(fileSystemItem, EventArgs.Empty);
            }
            else if (fileSystemItem.IsFile)
            {
                try
                {
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo(fileSystemItem.Path)
                    {
                        WorkingDirectory = System.IO.Path.GetDirectoryName(fileSystemItem.Path),
                        UseShellExecute = true,
                    };
                    System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("FileView.HandleItemDoubleClick: Can not open file " + fileSystemItem.Path);
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                }
            }
        }

        private void ImageItem_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateImage(sender);
        }

        private void ImageItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged && !e.HeightChanged) return;
            UpdateImage(sender);
        }

        private void ImageItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is Image image)) return;
            if (!(image.DataContext is ImageItem imageItem)) return;
            Thumbnails.DeprioritizeLoadImage(imageItem);
        }

        private void UpdateImage(object sender)
        {
            if (!(sender is Image image)) return;
            if (!(image.DataContext is ImageItem imageItem)) return;
            if (image.IsLoaded)
            {
                Thumbnails.LoadImage(imageItem, (int)image.Width, (int)image.Height);
            }
            else
            {
                image.Loaded -= Image_Loaded;
                image.Loaded += Image_Loaded;
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is Image image)) return;
            if (!(image.DataContext is ImageItem imageItem)) return;
            Thumbnails.LoadImage(imageItem, (int)image.Width, (int)image.Height);
        }

        private void FileViewListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!(sender is ListBox listBox) || !(UIHelper.UIHelper.GetVisualChild<ScrollViewer>(listBox) is ScrollViewer scrollViewer)) return;

            if (Math.Abs(e.ExtentHeightChange) > 0.0)
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible ? RelativeScrollPosition.X * scrollViewer.ScrollableWidth : 0);
            }
            if (Math.Abs(e.ExtentWidthChange) > 0.0)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? RelativeScrollPosition.Y * scrollViewer.ScrollableHeight : 0);
            }
            Point relativeScrollPosition;
            relativeScrollPosition.X = (scrollViewer.ScrollableWidth == 0) ? 0 : (scrollViewer.HorizontalOffset / scrollViewer.ScrollableWidth);
            relativeScrollPosition.Y = (scrollViewer.ScrollableHeight == 0) ? 0 : (scrollViewer.VerticalOffset / scrollViewer.ScrollableHeight);
            RelativeScrollPosition = relativeScrollPosition;
        }
    }
}
