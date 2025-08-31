using System.ComponentModel.DataAnnotations;

namespace SMA.Api.Requests.DevicesControllerRequests;

public class DeviceCreateRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;
}
