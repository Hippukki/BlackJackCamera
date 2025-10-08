using BlackJackCamera.Models;

namespace BlackJackCamera.Interfaces
{
    /// <summary>
    /// Сервис для взаимодействия с backend API детекции объектов
    /// </summary>
    public interface IDetectionApiService
    {
        /// <summary>
        /// Отправляет изображение на backend для распознавания объектов
        /// </summary>
        /// <param name="imageStream">Поток с изображением</param>
        /// <returns>Результат детекции</returns>
        Task<DetectionResponseDto> DetectObjectsAsync(Stream imageStream);

        /// <summary>
        /// Проверяет доступность backend API
        /// </summary>
        /// <returns>True если API доступен</returns>
        Task<bool> CheckHealthAsync();
    }
}
