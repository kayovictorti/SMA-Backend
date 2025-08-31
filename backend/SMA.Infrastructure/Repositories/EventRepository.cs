using Microsoft.EntityFrameworkCore;
using SMA.Application.Interfaces;
using SMA.Domain.Entities;
using SMA.Infrastructure.Persistence;

namespace SMA.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _db;
        public EventRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Event ev, CancellationToken ct)
        {
            _db.Events.Add(ev);
            await _db.SaveChangesAsync(ct);
        }

        public Task<List<Event>> GetLatestAsync(int take, CancellationToken ct) =>
            _db.Events.AsNoTracking()
                      .OrderByDescending(e => e.OccurredAt)
                      .Take(take)
                      .ToListAsync(ct);

        public Task<List<Event>> GetByDeviceAsync(long deviceId, int take, CancellationToken ct) =>
            _db.Events.AsNoTracking()
                      .Where(e => e.DeviceId == deviceId)
                      .OrderByDescending(e => e.OccurredAt)
                      .Take(take)
                      .ToListAsync(ct);
    }
}
