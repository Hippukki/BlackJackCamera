namespace BlackJackCamera.Api.Models
{
    /// <summary>
    /// Ответ API с результатами детекции
    /// </summary>
    public class DetectionResponse
    {
        /// <summary>
        /// Список обнаруженных объектов
        /// </summary>
        public List<Detection> Detections { get; set; } = new();

        /// <summary>
        /// Время обработки в миллисекундах
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        /// <summary>
        /// Статус выполнения
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Сообщение об ошибке (если есть)
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
