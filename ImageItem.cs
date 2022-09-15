using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCabinet
{
    public class ImageItem : FileSystemItem
    {
        public ImageItem(FileInfo fileSystemInfo) : base(fileSystemInfo)
        {
        }
    }
}
