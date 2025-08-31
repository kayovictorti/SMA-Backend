using SMA.Domain.Entities;

namespace SMA.Application.Interfaces;

public interface IEventRepository
{
    Task AddAsync(Event ev, CancellationToken ct);
    Task<List<Event>> GetLatestAsync(int take, CancellationToken ct);
    Task<List<Event>> GetByDeviceAsync(long deviceId, int take, CancellationToken ct);
}
