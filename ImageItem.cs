using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageCabinet
{
    public class ImageItem : FileSystemItem
    {
        private static readonly DependencyPropertyKey BitmapPropertyKey = DependencyProperty.RegisterReadOnly("Bitmap", typeof(ImageSource), typeof(ImageItem), new PropertyMetadata());
        public static readonly DependencyProperty BitmapProperty = BitmapPropertyKey.DependencyProperty;
        public ImageSource Bitmap
        {
            get { return (ImageSource)GetValue(BitmapProperty); }
            private set { SetValue(BitmapPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsLoadingPropertyKey = DependencyProperty.RegisterReadOnly("IsLoading", typeof(bool), typeof(ImageItem), new PropertyMetadata(true));
        public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            private set { SetValue(IsLoadingPropertyKey, value); }
        }

        public ImageItem(FileInfo fileSystemInfo) : base(fileSystemInfo)
        {
        }

        public void SetImage(BitmapSource? bitmap)
        {
            IsLoading = false;
            if (bitmap == null) return;
            Bitmap = bitmap;
        }
    }
}
