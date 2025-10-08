using BlackJackCamera.Api.Interfaces;
using BlackJackCamera.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlackJackCamera.Api.Controllers
{
    /// <summary>
    /// Контроллер для детекции объектов на изображениях
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DetectionController : ControllerBase
    {
        private readonly IObjectDetectionService _detectionService;
        private readonly IImageProcessor _imageProcessor;
        private readonly ILogger<DetectionController> _logger;

        public DetectionController(
            IObjectDetectionService detectionService,
            IImageProcessor imageProcessor,
            ILogger<DetectionController> logger)
        {
            _detectionService = detectionService;
            _imageProcessor = imageProcessor;
            _logger = logger;
        }

        /// <summary>
        /// Распознает объекты на загруженном изображении
        /// </summary>
        /// <param name="file">Изображение в формате JPEG/PNG (рекомендуется JPEG с качеством 70-80)</param>
        /// <returns>Список распознанных объектов с координатами и уверенностью</returns>
        /// <response code="200">Распознавание выполнено успешно</response>
        /// <response code="400">Неверный формат файла или файл не предоставлен</response>
        /// <response code="500">Ошибка обработки изображения</response>
        [HttpPost("detect")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DetectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DetectionResponse>> DetectObjects(IFormFile file)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Валидация входных данных
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file provided for detection");
                    return BadRequest(new DetectionResponse
                    {
                        Success = false,
                        ErrorMessage = "Файл не предоставлен"
                    });
                }

                // Проверка размера файла (макс 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    _logger.LogWarning("File too large: {Size} bytes", file.Length);
                    return BadRequest(new DetectionResponse
                    {
                        Success = false,
                        ErrorMessage = "Размер файла превышает 10MB"
                    });
                }

                // Проверка типа файла
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    _logger.LogWarning("Invalid content type: {ContentType}", file.ContentType);
                    return BadRequest(new DetectionResponse
                    {
                        Success = false,
                        ErrorMessage = "Поддерживаются только JPEG и PNG форматы"
                    });
                }

                _logger.LogInformation("Processing image: {FileName}, Size: {Size} bytes, Type: {ContentType}",
                    file.FileName, file.Length, file.ContentType);

                // Обработка изображения
                using var stream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Изменение размера и конвертация в тензор
                var bitmap = _imageProcessor.ResizeImage(memoryStream, 640, 640);
                var tensor = _imageProcessor.ConvertToTensor(bitmap);

                // Детекция объектов
                var detections = _detectionService.DetectObjects(tensor);

                stopwatch.Stop();

                _logger.LogInformation("Detection completed. Found {Count} objects in {Time}ms",
                    detections.Count, stopwatch.ElapsedMilliseconds);

                return Ok(new DetectionResponse
                {
                    Detections = detections,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error processing image");

                return StatusCode(500, new DetectionResponse
                {
                    Success = false,
                    ErrorMessage = $"Ошибка обработки изображения: {ex.Message}",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Проверка состояния сервиса
        /// </summary>
        /// <returns>Статус готовности сервиса</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            var classNames = _detectionService.GetClassNames();
            return Ok(new
            {
                Status = "healthy",
                Message = "Object detection service is ready",
                ClassesCount = classNames.Length,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
