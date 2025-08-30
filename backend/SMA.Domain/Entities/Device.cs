namespace SMA.Domain.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string IntegrationId { get; set; } = string.Empty;
}
