namespace SMA.Domain.Entities;

public class Event : BaseEntity
{
    public long DeviceId { get; set; }
    public Device? Device { get; set; } 
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public bool IsAlarm { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
