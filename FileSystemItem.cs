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
        public string Name { get { return FileSystemInfo != null ? FileSystemInfo.Name : string.Empty; } }
        public virtual string Path { get { return FileSystemInfo != null ? FileSystemInfo.FullName : string.Empty; } }

        public FileSystemItem(FileSystemInfo fileSystemInfo)
        {
            FileSystemInfo = fileSystemInfo;
        }
    }
}
