namespace SMA.Api.Responses;

public class EventResponse
{
    public long Id { get; set; }
    public long DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public bool IsAlarm { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreationDate { get; set; }
}
