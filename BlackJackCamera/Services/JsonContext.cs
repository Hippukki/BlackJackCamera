using BlackJackCamera.Models;
using System.Text.Json.Serialization;

namespace BlackJackCamera.Services
{
    /// <summary>
    /// JSON Source Generator контекст для оптимизации и совместимости с trimming
    /// </summary>
    [JsonSerializable(typeof(DetectionResponseDto))]
    [JsonSerializable(typeof(DetectionDto))]
    [JsonSerializable(typeof(List<DetectionDto>))]
    internal partial class JsonContext : JsonSerializerContext
    {
    }
}
