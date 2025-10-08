namespace BlackJackCamera.Models
{
    /// <summary>
    /// DTO для результата детекции объекта с backend API
    /// </summary>
    public class DetectionDto
    {
        /// <summary>
        /// Координата X центра bounding box
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Координата Y центра bounding box
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Ширина bounding box
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Высота bounding box
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Уверенность детекции (0-1)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// ID класса обнаруженного объекта
        /// </summary>
        public int ClassId { get; set; }

        /// <summary>
        /// Название класса объекта
        /// </summary>
        public string? ClassName { get; set; }
    }
}
