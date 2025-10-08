using BlackJackCamera.Api.Models;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BlackJackCamera.Api.Interfaces
{
    /// <summary>
    /// Сервис для детекции объектов с использованием YOLOv8
    /// </summary>
    public interface IObjectDetectionService
    {
        /// <summary>
        /// Инициализирует сервис: загружает модель и метки классов
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Выполняет детекцию объектов на изображении
        /// </summary>
        /// <param name="input">Тензор изображения размером [1, 3, 640, 640]</param>
        /// <returns>Список обнаруженных объектов после применения NMS</returns>
        List<Detection> DetectObjects(DenseTensor<float> input);

        /// <summary>
        /// Возвращает массив названий классов объектов
        /// </summary>
        /// <returns>Массив строк с названиями классов</returns>
        string[] GetClassNames();
    }
}
