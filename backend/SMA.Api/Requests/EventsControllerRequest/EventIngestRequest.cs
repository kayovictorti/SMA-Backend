using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SMA.Api.Requests;

public class EventIngestRequest
{
    [Required]
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [Required]
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [Required]
    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }

    [JsonPropertyName("isAlarm")]
    public bool IsAlarm { get; set; }
}
