using BlackJackCamera.Api.Interfaces;
using BlackJackCamera.Api.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BlackJackCamera.Api.Services
{
    /// <summary>
    /// Реализация сервиса детекции объектов с использованием YOLOv8
    /// </summary>
    public class ObjectDetectionService : IObjectDetectionService
    {
        private readonly IModelLoaderService _modelLoader;
        private readonly ILogger<ObjectDetectionService> _logger;
        private InferenceSession? _session;
        private string[] _classNames = Array.Empty<string>();

        private const float ConfidenceThreshold = 0.25f;
        private const float IouThreshold = 0.45f;
        private const string ModelFileName = "yolov8x-oiv7.onnx";
        private const string LabelsFileName = "labels.txt";

        /// <summary>
        /// Инициализирует новый экземпляр класса ObjectDetectionService
        /// </summary>
        /// <param name="modelLoader">Сервис загрузки моделей</param>
        /// <param name="logger">Логгер</param>
        public ObjectDetectionService(IModelLoaderService modelLoader, ILogger<ObjectDetectionService> logger)
        {
            _modelLoader = modelLoader;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing object detection service...");
                _session = await _modelLoader.LoadModelAsync(ModelFileName);
                _classNames = await _modelLoader.LoadLabelsAsync(LabelsFileName);
                _logger.LogInformation("Object detection service initialized successfully. Classes: {Count}", _classNames.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize object detection service");
                throw;
            }
        }

        /// <inheritdoc/>
        public string[] GetClassNames() => _classNames;

        /// <inheritdoc/>
        public List<Detection> DetectObjects(DenseTensor<float> input)
        {
            if (_session == null)
                throw new InvalidOperationException("Service not initialized. Call InitializeAsync first.");

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", input)
            };

            using var results = _session.Run(inputs);
            var output = results.First().AsTensor<float>();

            var detections = ParseDetections(output);
            var filteredDetections = ApplyNMS(detections, IouThreshold);

            // Добавляем названия классов
            foreach (var detection in filteredDetections)
            {
                if (detection.ClassId >= 0 && detection.ClassId < _classNames.Length)
                {
                    detection.ClassName = _classNames[detection.ClassId];
                }
            }

            return filteredDetections;
        }

        /// <summary>
        /// Парсит выходные данные YOLOv8 модели в список детекций
        /// </summary>
        /// <param name="output">Тензор выхода модели формата [1, num_classes + 4, num_predictions]</param>
        /// <returns>Список детекций, прошедших фильтрацию по порогу уверенности</returns>
        private List<Detection> ParseDetections(Tensor<float> output)
        {
            var detections = new List<Detection>();
            var shape = output.Dimensions.ToArray();

            int numPredictions = shape[2];
            int numClasses = _classNames.Length;

            for (int i = 0; i < numPredictions; i++)
            {
                float x = output[0, 0, i];
                float y = output[0, 1, i];
                float w = output[0, 2, i];
                float h = output[0, 3, i];

                float maxConfidence = 0;
                int maxClassId = 0;

                for (int c = 0; c < numClasses; c++)
                {
                    float confidence = output[0, 4 + c, i];
                    if (confidence > maxConfidence)
                    {
                        maxConfidence = confidence;
                        maxClassId = c;
                    }
                }

                if (maxConfidence >= ConfidenceThreshold)
                {
                    detections.Add(new Detection
                    {
                        X = x,
                        Y = y,
                        Width = w,
                        Height = h,
                        Confidence = maxConfidence,
                        ClassId = maxClassId
                    });
                }
            }

            return detections;
        }

        /// <summary>
        /// Применяет Non-Maximum Suppression (NMS) для удаления перекрывающихся детекций
        /// </summary>
        /// <param name="detections">Список детекций</param>
        /// <param name="iouThreshold">Порог IoU для фильтрации (обычно 0.45)</param>
        /// <returns>Отфильтрованный список детекций</returns>
        private static List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
        {
            var sorted = detections.OrderByDescending(d => d.Confidence).ToList();
            var results = new List<Detection>();

            while (sorted.Count > 0)
            {
                var best = sorted[0];
                results.Add(best);
                sorted.RemoveAt(0);

                sorted = sorted.Where(d => CalculateIoU(best, d) < iouThreshold).ToList();
            }

            return results;
        }

        /// <summary>
        /// Вычисляет Intersection over Union (IoU) между двумя детекциями
        /// </summary>
        /// <param name="a">Первая детекция</param>
        /// <param name="b">Вторая детекция</param>
        /// <returns>Значение IoU в диапазоне [0, 1]</returns>
        private static float CalculateIoU(Detection a, Detection b)
        {
            float x1A = a.X - a.Width / 2;
            float y1A = a.Y - a.Height / 2;
            float x2A = a.X + a.Width / 2;
            float y2A = a.Y + a.Height / 2;

            float x1B = b.X - b.Width / 2;
            float y1B = b.Y - b.Height / 2;
            float x2B = b.X + b.Width / 2;
            float y2B = b.Y + b.Height / 2;

            float intersectX1 = Math.Max(x1A, x1B);
            float intersectY1 = Math.Max(y1A, y1B);
            float intersectX2 = Math.Min(x2A, x2B);
            float intersectY2 = Math.Min(y2A, y2B);

            float intersectWidth = Math.Max(0, intersectX2 - intersectX1);
            float intersectHeight = Math.Max(0, intersectY2 - intersectY1);
            float intersectArea = intersectWidth * intersectHeight;

            float areaA = a.Width * a.Height;
            float areaB = b.Width * b.Height;

            float unionArea = areaA + areaB - intersectArea;
            return unionArea > 0 ? intersectArea / unionArea : 0;
        }
    }
}
