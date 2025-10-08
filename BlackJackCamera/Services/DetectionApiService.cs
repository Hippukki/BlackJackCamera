using BlackJackCamera.Interfaces;
using BlackJackCamera.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlackJackCamera.Services
{
    /// <summary>
    /// Реализация сервиса для взаимодействия с backend API детекции
    /// </summary>
    public class DetectionApiService : IDetectionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DetectionApiService> _logger;
        private readonly string _apiBaseUrl;

        public DetectionApiService(HttpClient httpClient, IConfiguration configuration, ILogger<DetectionApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Получаем URL из конфигурации
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <inheritdoc/>
        public async Task<DetectionResponseDto> DetectObjectsAsync(Stream imageStream)
        {
            try
            {
                _logger.LogInformation("Sending image to backend API for detection...");

                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(imageStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(streamContent, "file", "photo.jpg");

                var response = await _httpClient.PostAsync("/api/detection/detect", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API returned error: {StatusCode}, {Content}", response.StatusCode, errorContent);

                    return new DetectionResponseDto
                    {
                        Success = false,
                        ErrorMessage = $"Ошибка API: {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize(json, JsonContext.Default.DetectionResponseDto);

                _logger.LogInformation("Detection completed. Found {Count} objects in {Time}ms",
                    result?.Detections.Count ?? 0, result?.ProcessingTimeMs ?? 0);

                return result ?? new DetectionResponseDto { Success = false, ErrorMessage = "Пустой ответ от сервера" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while calling detection API");
                return new DetectionResponseDto
                {
                    Success = false,
                    ErrorMessage = $"Ошибка сети: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while calling detection API");
                return new DetectionResponseDto
                {
                    Success = false,
                    ErrorMessage = "Превышено время ожидания ответа от сервера"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling detection API");
                return new DetectionResponseDto
                {
                    Success = false,
                    ErrorMessage = $"Неожиданная ошибка: {ex.Message}"
                };
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/detection/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed");
                return false;
            }
        }
    }
}
