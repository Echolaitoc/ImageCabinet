using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageCabinet
{
    internal class ThumbnailCache
    {
        private struct ImageInfo
        {
            public ImageItem ImageItem;
            public int MaxWidth;
            public int MaxHeight;

            public ImageInfo(ImageItem imageItem, int maxWidth, int maxHeight)
            {
                ImageItem = imageItem;
                MaxWidth = maxWidth;
                MaxHeight = maxHeight;
            }

            public override int GetHashCode()
            {
                return ImageItem.Path.GetHashCode();
            }
        }

        private const double DEFAULT_DPI = 96.0;
        private Stack<ImageInfo> PendingThumbnails { get; } = new();
        private Dictionary<string, BitmapImage> ImageDictionary { get; } = new();
        private BackgroundWorker LoadImagesBackgroundWorker { get; } = new();

        public ThumbnailCache()
        {
            LoadImagesBackgroundWorker.WorkerSupportsCancellation = true;
            LoadImagesBackgroundWorker.DoWork += UpdateBackgroundWorker;
        }

        private void UpdateBackgroundWorker(object? sender, DoWorkEventArgs e)
        {
            while (PendingThumbnails.Count > 0 && !LoadImagesBackgroundWorker.CancellationPending)
            {
                var imageInfo = PendingThumbnails.Pop();
                var path = imageInfo.ImageItem?.Path;
                if (LoadImagesBackgroundWorker.CancellationPending || !IsImageInfoStillValid(imageInfo, path))
                {
                    continue;
                }
                var bytes = GetImageBytes(imageInfo);
                if (bytes != null)
                {
                    imageInfo.ImageItem?.Dispatcher.BeginInvoke(() =>
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = new MemoryStream(bytes);
                        bitmap.EndInit();
                        if (!LoadImagesBackgroundWorker.CancellationPending && !ImageDictionary.ContainsKey(path == null ? string.Empty : path) && IsImageInfoStillValid(imageInfo, path))
                        {
                            ImageDictionary.Add(path, bitmap);
                            imageInfo.ImageItem.SetImage(bitmap);
                        }
                    });
                }
                else
                {
                    imageInfo.ImageItem?.Dispatcher.BeginInvoke(() =>
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
                    using (var imageStream = File.OpenRead(imageInfo.ImageItem.Path))
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
                    }

                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
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
        }

        public void LoadImage(ImageItem imageItem, int maxWidth, int maxHeight)
        {
            if (ImageDictionary.ContainsKey(imageItem.Path))
            {
                imageItem.SetImage(ImageDictionary[imageItem.Path]);
                return;
            }
            var imageInfo = new ImageInfo(imageItem, maxWidth, maxHeight);
            if (!PendingThumbnails.Contains(imageInfo))
            {
                PendingThumbnails.Push(imageInfo);
            }
            if (!LoadImagesBackgroundWorker.IsBusy)
            {
                LoadImagesBackgroundWorker.RunWorkerAsync();
            }
        }
    }
}
