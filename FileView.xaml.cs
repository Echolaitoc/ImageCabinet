﻿using System;
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
        }
    }
}