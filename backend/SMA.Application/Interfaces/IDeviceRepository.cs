using SMA.Domain.Entities;

namespace SMA.Application.Interfaces;

public interface IDeviceRepository
{
    Task AddAsync(Device device, CancellationToken ct);
    Task UpdateAsync(Device device, CancellationToken ct);
    Task<Device?> GetByIdAsync(long id, CancellationToken ct);
    Task<List<Device>> GetAllAsync(CancellationToken ct);
    Task<Device?> GetByIdForUpdateAsync(long id, CancellationToken ct);
    Task DeleteAsync(Device device, CancellationToken ct);
}

