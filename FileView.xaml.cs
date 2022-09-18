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

        public event EventHandler OnItemDoubleClick;
        public ICommand ItemDoubleClickCommand { get; set; } = new RoutedUICommand();

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
            FillFileSystemItems(ref fileView, fileView.Path, true, true);
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
                        var directory = new FileSystemItem(new DirectoryInfo(dir));
                        directory.OnDoubleClick += (o, arg) =>
                        {
                            if (fw.ItemDoubleClickCommand != null && fw.ItemDoubleClickCommand.CanExecute(directory))
                            {
                                fw.ItemDoubleClickCommand.Execute(directory);
                            }
                        };
                        fileView.FileSystemItems.Add(directory);
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
                        var imageFile = new ImageItem(fileInfo);
                        imageFile.OnDoubleClick += (o, arg) =>
                        {
                            if (fw.ItemDoubleClickCommand != null && fw.ItemDoubleClickCommand.CanExecute(imageFile))
                            {
                                fw.ItemDoubleClickCommand.Execute(imageFile);
                            }
                        };
                        fileView.FileSystemItems.Add(imageFile);
                    }
                    else
                    {
                        var genericFile = new FileSystemItem(fileInfo);
                        genericFile.OnDoubleClick += (o, arg) =>
                        {
                            if (fw.ItemDoubleClickCommand != null && fw.ItemDoubleClickCommand.CanExecute(genericFile))
                            {
                                fw.ItemDoubleClickCommand.Execute(genericFile);
                            }
                        };
                        fileView.FileSystemItems.Add(genericFile);
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
    }
}
