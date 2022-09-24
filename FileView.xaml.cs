using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageCabinet
{
    public partial class FileView : UserControl
    {
        private static List<string> SUPPORTED_FILE_EXTENSIONS = new()
        {
            ".jpg",
            ".png",
        };

        public event EventHandler? OnItemDoubleClick;
        public ICommand ItemDoubleClickCommand { get; set; } = new RoutedUICommand();
        private ThumbnailCache Thumbnails { get; set; } = new();

        public ObservableCollection<FileSystemItem> FileSystemItems { get; set; } = new();

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(FileView), new FrameworkPropertyMetadata()
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
            if (!(e.NewValue is string strPath) || !(d is FileView fileView)) return;
            fileView.Thumbnails.ClearCache();
            fileView.Dispatcher.BeginInvoke(() =>
            {
                FillFileSystemItems(ref fileView, fileView.Path, true, true);
            });
        }

        private static void FillFileSystemItems(ref FileView fileView, string path, bool clearFileViewFirst, bool addFolders)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            if (clearFileViewFirst)
            {
                fileView.FileSystemItems.Clear();
            }

            try
            {
                var dirs = Directory.EnumerateDirectories(path);
                var fw = fileView;
                foreach (var dir in dirs)
                {
                    if (addFolders)
                    {
                        fileView.FileSystemItems.Add(new FileSystemItem(new DirectoryInfo(dir)));
                    }
                    if (fileView.IncludeFilesInSubfolder)
                    {
                        FillFileSystemItems(ref fileView, dir, false, false);
                    }
                }
                var files = Directory.EnumerateFiles(path);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (SUPPORTED_FILE_EXTENSIONS.Any(ext => file.EndsWith(ext)))
                    {
                        fileView.FileSystemItems.Add(new ImageItem(fileInfo));
                    }
                    else
                    {
                        fileView.FileSystemItems.Add(new FileSystemItem(fileInfo));
                    }
                }
            }
            catch (Exception)
            {

            }
            fileView.ScrollToTop();
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

        private static void OnIncludeFilesInSubfolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FileView fileView)) return;
            FillFileSystemItems(ref fileView, fileView.Path, true, true);
        }

        public FileView()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(ItemDoubleClickCommand, HandleItemDoubleClick));
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
    }
}
