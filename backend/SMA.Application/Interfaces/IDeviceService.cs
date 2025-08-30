using SMA.Application.DTOs;
using SMA.Domain.Entities;

namespace SMA.Application.Interfaces;

public interface IDeviceService
{
    Task<Device> CreateAsync(DeviceDto dto, CancellationToken ct);
    Task<List<Device?>> GetAllAsync(CancellationToken ct);
    Task<Device?> GetByIdAsync(long id, CancellationToken ct);
}
