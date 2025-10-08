using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace BlackJackCamera.Api.Interfaces
{
    /// <summary>
    /// Сервис для обработки изображений
    /// </summary>
    public interface IImageProcessor
    {
        /// <summary>
        /// Изменяет размер изображения
        /// </summary>
        /// <param name="imageStream">Поток с исходным изображением</param>
        /// <param name="width">Целевая ширина</param>
        /// <param name="height">Целевая высота</param>
        /// <returns>Измененное изображение</returns>
        SKBitmap ResizeImage(Stream imageStream, int width, int height);

        /// <summary>
        /// Конвертирует изображение в тензор для ML модели
        /// </summary>
        /// <param name="bitmap">Исходное изображение</param>
        /// <returns>Тензор формата [1, 3, height, width] с нормализованными значениями RGB (0-1)</returns>
        DenseTensor<float> ConvertToTensor(SKBitmap bitmap);
    }
}
