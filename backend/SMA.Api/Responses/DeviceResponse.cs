namespace SMA.Api.Responses;

public class DeviceResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string IntegrationId { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public DateTime? DeletionDate { get; set; }
}
