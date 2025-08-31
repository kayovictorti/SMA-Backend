namespace SMA.Application.DTOs;

public class EventDto
{
    public string IntegrationId { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public bool IsAlarm { get; set; }
    public DateTime OccurredAt { get; set; }
}
