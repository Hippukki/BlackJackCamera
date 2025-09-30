using BlackJackCamera.Interfaces;
using Microsoft.ML.OnnxRuntime;
using System.Text.Json;

namespace BlackJackCamera.Services
{
    /// <summary>
    /// Реализация сервиса загрузки ML моделей и связанных файлов
    /// </summary>
    public class ModelLoaderService : IModelLoaderService
    {
        /// <inheritdoc/>
        public async Task<InferenceSession> LoadModelAsync(string modelFileName)
        {
            var modelPath = Path.Combine(FileSystem.AppDataDirectory, modelFileName);

            if (!File.Exists(modelPath))
            {
                await CopyFromAppPackageAsync(modelFileName, modelPath);
            }

            return new InferenceSession(modelPath);
        }

        /// <inheritdoc/>
        public async Task<string[]> LoadLabelsAsync(string labelsFileName)
        {
            var labelsPath = Path.Combine(FileSystem.AppDataDirectory, labelsFileName);

            if (!File.Exists(labelsPath))
            {
                await CopyFromAppPackageAsync(labelsFileName, labelsPath);
            }

            var json = await File.ReadAllTextAsync(labelsPath);
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }

        /// <summary>
        /// Копирует файл из пакета приложения в файловую систему устройства
        /// </summary>
        /// <param name="fileName">Имя файла в пакете</param>
        /// <param name="destinationPath">Путь назначения</param>
        private static async Task CopyFromAppPackageAsync(string fileName, string destinationPath)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var fileStream = File.Create(destinationPath);
            await stream.CopyToAsync(fileStream);
        }
    }
}