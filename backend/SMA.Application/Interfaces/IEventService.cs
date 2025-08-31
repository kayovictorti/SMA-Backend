using SMA.Application.DTOs;
using SMA.Domain.Entities;

namespace SMA.Application.Interfaces;

public interface IEventService
{
    Task<Event> IngestAsync(EventDto dto, CancellationToken ct);
    Task<List<Event>> GetLatestAsync(int take, CancellationToken ct);
    Task<List<Event>> GetByDeviceAsync(long deviceId, int take, CancellationToken ct);
}
