using BlackJackCamera.Api.Interfaces;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace BlackJackCamera.Api.Services
{
    /// <summary>
    /// Реализация сервиса обработки изображений
    /// </summary>
    public class ImageProcessor : IImageProcessor
    {
        /// <inheritdoc/>
        public SKBitmap ResizeImage(Stream imageStream, int width, int height)
        {
            var bitmap = SKBitmap.Decode(imageStream);
            return bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        }

        /// <inheritdoc/>
        public DenseTensor<float> ConvertToTensor(SKBitmap bitmap)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, bitmap.Height, bitmap.Width });

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    // Нормализация RGB значений к диапазону [0, 1]
                    tensor[0, 0, y, x] = pixel.Red / 255f;
                    tensor[0, 1, y, x] = pixel.Green / 255f;
                    tensor[0, 2, y, x] = pixel.Blue / 255f;
                }
            }

            return tensor;
        }
    }
}
