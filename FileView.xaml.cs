using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public event EventHandler? OnItemDoubleClick;
        public ICommand ItemDoubleClickCommand { get; set; } = new RoutedUICommand();
        private ThumbnailCache Thumbnails { get; set; } = new();
        private BackgroundWorker LoadFilesBackgroundWorker { get; } = new();

        public ObservableCollection<FileSystemItem> FileSystemItems { get; set; } = new();
        private Queue<string> PendingPaths { get; set; } = new();
        private Queue<Tuple<IEnumerable<string>, bool>> PendingDirectories { get; set; } = new();
        private Queue<IEnumerable<string>> PendingFiles { get; set; } = new();
        private bool CurrentlyGeneratingPendingItems { get; set; } = false;

        private Point RelativeScrollPosition { get; set; } = new(0.0, 0.0);

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
            fileView.EnqueuePendingPath(strPath);
        }

        private void ResetFileSystemItems()
        {
            Thumbnails.ClearCache();
            CancelLoadFilesBackgroundWorker();
            FileSystemItems.Clear();
            PendingPaths.Clear();
            PendingDirectories.Clear();
            PendingFiles.Clear();
        }

        private void LoadFilesBackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (PendingPaths.Count > 0 && !LoadFilesBackgroundWorker.CancellationPending)
            {
                try
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
                    LoadFilesBackgroundWorker.ReportProgress(1);
                }
                catch { }
            }
        }

        private void LoadFilesBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
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
            catch { }
        }

        private void LoadFilesBackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (PendingPaths.Count > 0)
            {
                IncludeFilesInSubfolderForBackgroundWorker = IncludeFilesInSubfolder;
                PathForBackgroundWorker = Path;
                LoadFilesBackgroundWorker.RunWorkerAsync();
            }
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
            }
        }

        private void AddSubfoldersToPendingPaths(string path)
        {
            if (!Directory.Exists(path)) return;
            var dirs = Directory.EnumerateDirectories(path);
            foreach (var dir in dirs)
            {
                EnqueuePendingPath(dir);
            }
        }

        private void EnqueuePendingPath(string path)
        {
            if (PendingPaths.Contains(path)) return;
            PendingPaths.Enqueue(path);
            if (!LoadFilesBackgroundWorker.IsBusy)
            {
                IncludeFilesInSubfolderForBackgroundWorker = IncludeFilesInSubfolder;
                PathForBackgroundWorker = Path;
                LoadFilesBackgroundWorker.RunWorkerAsync();
            }
        }

        private void CancelLoadFilesBackgroundWorker()
        {
            if (LoadFilesBackgroundWorker.IsBusy)
            {
                LoadFilesBackgroundWorker.CancelAsync();
            }
        }

        public FileView()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(ItemDoubleClickCommand, HandleItemDoubleClick));

            LoadFilesBackgroundWorker.WorkerSupportsCancellation = true;
            LoadFilesBackgroundWorker.WorkerReportsProgress = true;
            LoadFilesBackgroundWorker.DoWork += LoadFilesBackgroundWorker_DoWork;
            LoadFilesBackgroundWorker.ProgressChanged += LoadFilesBackgroundWorker_ProgressChanged;
            LoadFilesBackgroundWorker.RunWorkerCompleted += LoadFilesBackgroundWorker_RunWorkerCompleted;
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
