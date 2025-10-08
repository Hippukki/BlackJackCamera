namespace BlackJackCamera.Interfaces
{
    /// <summary>
    /// Сервис для сжатия изображений перед отправкой на backend
    /// </summary>
    public interface IImageCompressionService
    {
        /// <summary>
        /// Сжимает изображение до оптимального размера для передачи по сети
        /// </summary>
        /// <param name="imageStream">Поток с исходным изображением</param>
        /// <param name="maxWidth">Максимальная ширина</param>
        /// <param name="maxHeight">Максимальная высота</param>
        /// <param name="quality">Качество JPEG (0-100)</param>
        /// <returns>Поток со сжатым изображением</returns>
        Task<Stream> CompressImageAsync(Stream imageStream, int maxWidth = 1920, int maxHeight = 1080, int quality = 75);
    }
}
