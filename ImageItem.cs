using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageCabinet
{
    public class ImageItem : FileSystemItem
    {
        private static readonly DependencyPropertyKey BitmapPropertyKey = DependencyProperty.RegisterReadOnly("Bitmap", typeof(ImageSource), typeof(FileView), new PropertyMetadata());
        public static readonly DependencyProperty BitmapProperty = BitmapPropertyKey.DependencyProperty;
        public ImageSource Bitmap
        {
            get { return (ImageSource)GetValue(BitmapProperty); }
            private set { SetValue(BitmapPropertyKey, value); }
        }

        private CancellationTokenSource? GenerateBitmapCancellationTokenSource;

        public ImageItem(FileInfo fileSystemInfo) : base(fileSystemInfo)
        {
        }

        public async void UpdateImageAsync(int maxWidth, int maxHeight)
        {
            try
            {
                if (GenerateBitmapCancellationTokenSource != null)
                {
                    GenerateBitmapCancellationTokenSource.Cancel();
                }
                GenerateBitmapCancellationTokenSource = new CancellationTokenSource();
                Bitmap = await Task.Run(() =>
                {
                    try
                    {
                        var bmp = UIHelper.UIHelper.GetDownscaledBitmapImage(Path, maxWidth, maxHeight);
                        if (bmp != null)
                        {
                            return bmp;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    return new BitmapImage();
                }, GenerateBitmapCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                if (GenerateBitmapCancellationTokenSource != null)
                {
                    GenerateBitmapCancellationTokenSource.Dispose();
                    GenerateBitmapCancellationTokenSource = null;
                }
            }
        }

        public void UpdateImage(int maxWidth, int maxHeight)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var bmp = UIHelper.UIHelper.GetDownscaledBitmapImage(Path, maxWidth, maxHeight);
                    if (bmp != null)
                    {
                        Bitmap = bmp;
                    }
                }
                catch (TaskCanceledException)
                {
                }
            });
        }
    }
}
