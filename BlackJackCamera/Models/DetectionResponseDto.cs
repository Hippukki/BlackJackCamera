namespace BlackJackCamera.Models
{
    /// <summary>
    /// DTO для ответа API с результатами детекции
    /// </summary>
    public class DetectionResponseDto
    {
        /// <summary>
        /// Список обнаруженных объектов
        /// </summary>
        public List<DetectionDto> Detections { get; set; } = new();

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
