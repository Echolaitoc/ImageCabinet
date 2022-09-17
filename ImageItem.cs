using System.IO;

namespace ImageCabinet
{
    public class ImageItem : FileSystemItem
    {
        public string Path { get { return FileSystemInfo.FullName; } }

        public ImageItem(FileInfo fileSystemInfo) : base(fileSystemInfo)
        {
        }
    }
}
