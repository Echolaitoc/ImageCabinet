using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageCabinet
{
    public partial class FileView : UserControl
    {
        private class FileViewImageItemToBitmapImageConverter : System.Windows.Data.IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (!(value is Image image)) return DependencyProperty.UnsetValue;
                if (!(image.DataContext is ImageItem imageItem)) return DependencyProperty.UnsetValue;

                var bmp = UIHelper.UIHelper.GetDownscaledBitmapImage(imageItem.Path, (int)image.Width, (int)image.Height);
                if (bmp != null)
                {
                    return bmp;
                }
                return DependencyProperty.UnsetValue;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private static List<string> SUPPORTED_FILE_EXTENSIONS = new()
        {
            ".jpg",
            ".png",
        };

        public event EventHandler? OnItemDoubleClick;
        public ICommand ItemDoubleClickCommand { get; set; } = new RoutedUICommand();
        private FileViewImageItemToBitmapImageConverter ImageItemConverter = new();

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

        private bool TrueIfImageViaConverterFalseIfAsyncLoading { get; } = false;

        private void UpdateImage(object sender)
        {
            if (TrueIfImageViaConverterFalseIfAsyncLoading)
            {
                SetImageViaConverter(sender);
            }
            else
            {
                SetImageAsync(sender);
            }
        }

        private const int FALLBACK_IMAGE_SIZE = 100;

        private void SetImageViaConverter(object sender)
        {
            if (!(sender is Image image)) return;
            var fallback = UIHelper.Icon.GetImageSource("file-image-remove-outline", Colors.OrangeRed, FALLBACK_IMAGE_SIZE, FALLBACK_IMAGE_SIZE);
            var sourceBinding = new System.Windows.Data.Binding();
            sourceBinding.Source = image;
            sourceBinding.Converter = ImageItemConverter;
            sourceBinding.IsAsync = true;
            sourceBinding.Mode = System.Windows.Data.BindingMode.OneWay;
            sourceBinding.FallbackValue = fallback;
            sourceBinding.TargetNullValue = fallback;
            image.SetBinding(Image.SourceProperty, sourceBinding);
        }

        private void SetImageAsync(object sender)
        {
            if (!(sender is Image image)) return;
            if (!(image.DataContext is ImageItem imageItem)) return;
            imageItem.UpdateImage((int)image.Width, (int)image.Height);

            var fallback = UIHelper.Icon.GetImageSource("file-image-remove-outline", Colors.OrangeRed, FALLBACK_IMAGE_SIZE, FALLBACK_IMAGE_SIZE);
            var bitmapBinding = new System.Windows.Data.Binding();
            bitmapBinding.Source = imageItem;
            bitmapBinding.Path = new PropertyPath(ImageItem.BitmapProperty);
            bitmapBinding.IsAsync = true;
            bitmapBinding.Mode = System.Windows.Data.BindingMode.OneWay;
            bitmapBinding.FallbackValue = fallback;
            bitmapBinding.TargetNullValue = fallback;
            image.SetBinding(Image.SourceProperty, bitmapBinding);
        }
    }
}
