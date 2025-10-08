using Microsoft.ML.OnnxRuntime;

namespace BlackJackCamera.Api.Interfaces
{
    /// <summary>
    /// Сервис для загрузки ML моделей и связанных файлов
    /// </summary>
    public interface IModelLoaderService
    {
        /// <summary>
        /// Загружает ONNX модель из файловой системы
        /// </summary>
        /// <param name="modelFileName">Имя файла модели</param>
        /// <returns>Инициализированная сессия ONNX Runtime</returns>
        Task<InferenceSession> LoadModelAsync(string modelFileName);

        /// <summary>
        /// Загружает список меток классов из JSON файла
        /// </summary>
        /// <param name="labelsFileName">Имя файла с метками</param>
        /// <returns>Массив строк с названиями классов</returns>
        Task<string[]> LoadLabelsAsync(string labelsFileName);
    }
}
