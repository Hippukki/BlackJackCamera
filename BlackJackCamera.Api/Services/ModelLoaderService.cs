using BlackJackCamera.Api.Interfaces;
using Microsoft.ML.OnnxRuntime;
using System.Text.Json;

namespace BlackJackCamera.Api.Services
{
    /// <summary>
    /// Реализация сервиса загрузки ML моделей и связанных файлов
    /// </summary>
    public class ModelLoaderService : IModelLoaderService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ModelLoaderService> _logger;

        public ModelLoaderService(IWebHostEnvironment environment, ILogger<ModelLoaderService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<InferenceSession> LoadModelAsync(string modelFileName)
        {
            var modelPath = Path.Combine(_environment.ContentRootPath, "Resources", "Models", modelFileName);

            if (!File.Exists(modelPath))
            {
                _logger.LogError("Model file not found at: {ModelPath}", modelPath);
                throw new FileNotFoundException($"Model file not found: {modelFileName}", modelPath);
            }

            _logger.LogInformation("Loading model from: {ModelPath}", modelPath);

            return await Task.Run(() => new InferenceSession(modelPath));
        }

        /// <inheritdoc/>
        public async Task<string[]> LoadLabelsAsync(string labelsFileName)
        {
            var labelsPath = Path.Combine(_environment.ContentRootPath, "Resources", "Models", labelsFileName);

            if (!File.Exists(labelsPath))
            {
                _logger.LogError("Labels file not found at: {LabelsPath}", labelsPath);
                throw new FileNotFoundException($"Labels file not found: {labelsFileName}", labelsPath);
            }

            _logger.LogInformation("Loading labels from: {LabelsPath}", labelsPath);

            var json = await File.ReadAllTextAsync(labelsPath);
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
    }
}
