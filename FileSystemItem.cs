using System.IO;
using System.Windows.Media;

namespace ImageCabinet
{
    public class FileSystemItem
    {
        public FileSystemInfo FileSystemInfo { get; set; }
        public bool IsFile { get { return FileSystemInfo != null && FileSystemInfo is FileInfo; } }
        public bool IsDirectory { get { return FileSystemInfo != null && FileSystemInfo is DirectoryInfo; } }
        public string Name { get { return FileSystemInfo != null ? FileSystemInfo.Name : string.Empty; } }

        public FileSystemItem(FileSystemInfo fileSystemInfo)
        {
            FileSystemInfo = fileSystemInfo;
        }
    }
}
