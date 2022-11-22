using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Media.Imaging;

namespace ImageCabinet
{
    internal class ThumbnailCache
    {
        private struct ImageInfo
        {
            public ImageItem ImageItem;
            public BitmapImage? Bitmap = null;
            public int MaxWidth;
            public int MaxHeight;
            public bool RetryTimerIsElapsed = true;

            private Timer RetryTimer = new(500);

            public ImageInfo(ImageItem imageItem, int maxWidth, int maxHeight)
            {
                ImageItem = imageItem;
                MaxWidth = maxWidth;
                MaxHeight = maxHeight;
            }

            public void StartRetryTimer()
            {
                RetryTimer.Elapsed -= RetryTimer_Elapsed;
                RetryTimer.Elapsed += RetryTimer_Elapsed;
                RetryTimerIsElapsed = false;
                RetryTimer.Start();
            }

            private void RetryTimer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                RetryTimer.Stop();
                RetryTimerIsElapsed = true;
            }

            public override int GetHashCode()
            {
                return ImageItem.Path.GetHashCode();
            }
        }

        private const double DEFAULT_DPI = 96.0;
        private List<ImageInfo> PendingThumbnails { get; } = new();
        private Queue<ImageInfo> PendingThumbnailsLowPriority { get; } = new();
        private Queue<ImageInfo> PendingThumbnailsRetryPriority { get; } = new();
        private Dictionary<string, ImageInfo> ImageDictionary { get; } = new();
        private BackgroundWorker LoadImagesBackgroundWorker { get; } = new();

        public ThumbnailCache()
        {
            LoadImagesBackgroundWorker.WorkerSupportsCancellation = true;
            LoadImagesBackgroundWorker.DoWork += UpdateBackgroundWorker;
            LoadImagesBackgroundWorker.RunWorkerCompleted += BackgroundWorkerCompleted;
        }

        private void UpdateBackgroundWorker(object? sender, DoWorkEventArgs e)
        {
            while (PendingThumbnails.Any() || PendingThumbnailsLowPriority.Any() || PendingThumbnailsRetryPriority.Any() && !LoadImagesBackgroundWorker.CancellationPending)
            {
                ImageInfo? imageInfoNullable = null;
                if (PendingThumbnails.Any())
                {
                    imageInfoNullable = PendingThumbnails.Last();
                    PendingThumbnails.RemoveAt(PendingThumbnails.Count - 1);
                }
                else if (PendingThumbnailsLowPriority.Any())
                {
                    imageInfoNullable = PendingThumbnailsLowPriority.Dequeue();
                }
                else
                {
                    imageInfoNullable = PendingThumbnailsRetryPriority.Dequeue();
                    if (imageInfoNullable != null && !imageInfoNullable.Value.RetryTimerIsElapsed)
                    {
                        PendingThumbnailsRetryPriority.Enqueue(imageInfoNullable.Value);
                        continue;
                    }
                }
                var path = imageInfoNullable?.ImageItem.Path;
                if (LoadImagesBackgroundWorker.CancellationPending || imageInfoNullable == null || !IsImageInfoStillValid((ImageInfo)imageInfoNullable, path))
                {
                    continue;
                }
                var imageInfo = (ImageInfo)imageInfoNullable;
                var bytes = GetImageBytes(imageInfo);
                if (bytes != null)
                {
                    imageInfo.ImageItem.Dispatcher.BeginInvoke(() =>
                    {
                        BitmapImage bitmap = new();
                        bitmap.BeginInit();
                        bitmap.StreamSource = new MemoryStream(bytes);
                        bitmap.EndInit();
                        var pathValue = path == null ? string.Empty : path;
                        if (!LoadImagesBackgroundWorker.CancellationPending && IsImageInfoStillValid(imageInfo, pathValue))
                        {
                            imageInfo.Bitmap = bitmap;
                            ImageDictionary[pathValue] = imageInfo;
                            imageInfo.ImageItem.SetImage(bitmap);
                        }
                    });
                }
                else
                {
                    PendingThumbnailsRetryPriority.Enqueue(imageInfo);
                    imageInfo.StartRetryTimer();
                    imageInfo.ImageItem.Dispatcher.BeginInvoke(() =>
                    {
                        imageInfo.ImageItem.SetImage(null);
                    });
                }
            }
        }

        private byte[]? GetImageBytes(ImageInfo imageInfo)
        {
            if (imageInfo.ImageItem == null) return null;
            try
            {
                if (File.Exists(imageInfo.ImageItem.Path))
                {
                    int width = imageInfo.MaxWidth;
                    int height = imageInfo.MaxHeight;
                    double dpiX = DEFAULT_DPI;
                    double dpiY = DEFAULT_DPI;
                    using (var imageStream = new FileStream(imageInfo.ImageItem.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                        width = decoder.Frames[0].PixelWidth;
                        height = decoder.Frames[0].PixelHeight;
                        dpiX = decoder.Frames[0].DpiX;
                        dpiY = decoder.Frames[0].DpiY;
                        if (decoder.Thumbnail != null)
                        {
                            imageInfo.ImageItem.SetImage(decoder.Thumbnail);
                        }
                        if (decoder.Preview != null)
                        {
                            imageInfo.ImageItem.SetImage(decoder.Preview);
                        }
                        imageStream.Close();
                    }

                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = new Uri(imageInfo.ImageItem.Path);
                    double scale = 1.0;
                    if (width > 0 && width >= height)
                    {
                        if (Math.Round(dpiX) != DEFAULT_DPI)
                        {
                            scale = dpiX / DEFAULT_DPI;
                        }
                        bitmapImage.DecodePixelWidth = (int)(Math.Min(imageInfo.MaxWidth, width) * scale);
                    }
                    else if (height > 0)
                    {
                        if (Math.Round(dpiY) != DEFAULT_DPI)
                        {
                            scale = dpiY / DEFAULT_DPI;
                        }
                        bitmapImage.DecodePixelHeight = (int)(Math.Min(imageInfo.MaxHeight, height) * scale);
                    }
                    bitmapImage.EndInit();
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        private bool IsImageInfoStillValid(ImageInfo imageInfo, string? path)
        {
            return imageInfo.ImageItem != null && path != null && imageInfo.ImageItem.Path == path;
        }

        public void ClearCache()
        {
            LoadImagesBackgroundWorker.CancelAsync();
            ImageDictionary.Clear();
            PendingThumbnails.Clear();
            PendingThumbnailsLowPriority.Clear();
            PendingThumbnailsRetryPriority.Clear();
        }

        private void BackgroundWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if ((PendingThumbnails.Any() || PendingThumbnailsLowPriority.Any() || PendingThumbnailsRetryPriority.Any()) && !LoadImagesBackgroundWorker.IsBusy)
            {
                LoadImagesBackgroundWorker.RunWorkerAsync();
            }
        }

        public void LoadImage(ImageItem imageItem, int maxWidth, int maxHeight)
        {
            if (ImageDictionary.ContainsKey(imageItem.Path))
            {
                imageItem.SetImage(ImageDictionary[imageItem.Path].Bitmap);
                return;
            }
            var imageInfo = new ImageInfo(imageItem, maxWidth, maxHeight);
            if (!PendingThumbnails.Contains(imageInfo) && !PendingThumbnailsLowPriority.Contains(imageInfo) && !PendingThumbnailsRetryPriority.Contains(imageInfo))
            {
                PendingThumbnails.Add(imageInfo);
            }
            if (!LoadImagesBackgroundWorker.IsBusy)
            {
                LoadImagesBackgroundWorker.RunWorkerAsync();
            }
        }

        public void ReloadImage(ImageItem imageItem)
        {
            if (PendingThumbnails.Any(x => x.ImageItem.Path == imageItem.Path) || PendingThumbnailsLowPriority.Any(x => x.ImageItem.Path == imageItem.Path) || PendingThumbnailsRetryPriority.Any(x => x.ImageItem.Path == imageItem.Path))
            {
                return;
            }
            if (ImageDictionary.ContainsKey(imageItem.Path))
            {
                ImageInfo imageInfo = ImageDictionary[imageItem.Path];
                imageInfo.Bitmap = null;
                PendingThumbnails.Add(imageInfo);
                if (!LoadImagesBackgroundWorker.IsBusy)
                {
                    LoadImagesBackgroundWorker.RunWorkerAsync();
                }
            }
        }

        public void DeprioritizeLoadImage(ImageItem imageItem)
        {
            if (PendingThumbnails.FirstOrDefault(imageInfo => imageInfo.ImageItem.Path == imageItem.Path) is ImageInfo pendingImageInfo)
            {
                PendingThumbnails.Remove(pendingImageInfo);
                PendingThumbnailsLowPriority.Enqueue(pendingImageInfo);
            }
        }
    }
}
