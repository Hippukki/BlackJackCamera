using BlackJackCamera.Interfaces;
using SkiaSharp;

namespace BlackJackCamera.Services
{
    /// <summary>
    /// Реализация сервиса сжатия изображений
    /// </summary>
    public class ImageCompressionService : IImageCompressionService
    {
        /// <inheritdoc/>
        public async Task<Stream> CompressImageAsync(Stream imageStream, int maxWidth = 1920, int maxHeight = 1080, int quality = 75)
        {
            return await Task.Run(() =>
            {
                // Декодируем изображение
                using var originalBitmap = SKBitmap.Decode(imageStream);

                if (originalBitmap == null)
                    throw new InvalidOperationException("Не удалось декодировать изображение");

                // Вычисляем новые размеры с сохранением пропорций
                var (newWidth, newHeight) = CalculateNewDimensions(
                    originalBitmap.Width,
                    originalBitmap.Height,
                    maxWidth,
                    maxHeight);

                // Изменяем размер только если изображение больше максимальных размеров
                SKBitmap resizedBitmap;
                if (newWidth < originalBitmap.Width || newHeight < originalBitmap.Height)
                {
                    var samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
                    resizedBitmap = originalBitmap.Resize(
                        new SKImageInfo(newWidth, newHeight),
                        samplingOptions);
                }
                else
                {
                    resizedBitmap = originalBitmap;
                }

                // Кодируем в JPEG с заданным качеством
                using var image = SKImage.FromBitmap(resizedBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

                // Создаем новый поток с сжатым изображением
                var outputStream = new MemoryStream();
                data.SaveTo(outputStream);
                outputStream.Position = 0;

                if (resizedBitmap != originalBitmap)
                {
                    resizedBitmap.Dispose();
                }

                return outputStream;
            });
        }

        /// <summary>
        /// Вычисляет новые размеры изображения с сохранением пропорций
        /// </summary>
        private (int width, int height) CalculateNewDimensions(int width, int height, int maxWidth, int maxHeight)
        {
            if (width <= maxWidth && height <= maxHeight)
            {
                return (width, height);
            }

            var ratioX = (double)maxWidth / width;
            var ratioY = (double)maxHeight / height;
            var ratio = Math.Min(ratioX, ratioY);

            return ((int)(width * ratio), (int)(height * ratio));
        }
    }
}
