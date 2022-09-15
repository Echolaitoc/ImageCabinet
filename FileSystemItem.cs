using System.IO;

namespace ImageCabinet
{
    public class FileSystemItem
    {
        public FileSystemInfo FileSystemInfo { get; set; }

        public FileSystemItem(FileSystemInfo fileSystemInfo)
        {
            FileSystemInfo = fileSystemInfo;
        }
    }
}
