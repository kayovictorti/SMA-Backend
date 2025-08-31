using SMA.Application.DTOs;
using SMA.Domain.Entities;

namespace SMA.Application.Interfaces;

public interface IEventService
{
    Task<Event> IngestAsync(EventDto dto, CancellationToken ct);
}
