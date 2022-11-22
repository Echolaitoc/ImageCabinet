using System.IO;
using System.Windows;

namespace ImageCabinet
{
    public class FileSystemItem : DependencyObject
    {
        public FileSystemInfo FileSystemInfo { get; set; }
        public bool IsDirectory { get { return FileSystemInfo != null && FileSystemInfo is DirectoryInfo; } }
        public bool IsSubfolder { get; set; }
        public bool IsFile { get { return FileSystemInfo != null && FileSystemInfo is FileInfo; } }
        public bool IsImage { get { return IsFile && GetType() == typeof(ImageItem); } }
        public virtual string Path { get { return FileSystemInfo != null ? FileSystemInfo.FullName : string.Empty; } }

        private static readonly DependencyPropertyKey NamePropertyKey = DependencyProperty.RegisterReadOnly("Name", typeof(string), typeof(FileSystemItem), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty NameProperty = NamePropertyKey.DependencyProperty;
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            private set { SetValue(NamePropertyKey, value); }
        }

        public FileSystemItem(FileSystemInfo fileSystemInfo)
        {
            UpdateFileSystemInfo(fileSystemInfo);
        }

        public void UpdateFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            FileSystemInfo = fileSystemInfo;
            Name = FileSystemInfo != null ? FileSystemInfo.Name : string.Empty;
        }

        public override string? ToString()
        {
            return string.IsNullOrEmpty(Path) ? base.ToString() : Path;
        }
    }
}
