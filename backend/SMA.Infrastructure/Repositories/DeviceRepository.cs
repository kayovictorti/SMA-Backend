using Microsoft.EntityFrameworkCore;
using SMA.Application.Interfaces;
using SMA.Domain.Entities;
using SMA.Infrastructure.Persistence;

namespace SMA.Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _db;

        public DeviceRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Device device, CancellationToken ct)
        {
            _db.Devices.Add(device);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Device device, CancellationToken ct)
        {
            _db.Devices.Update(device);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<Device?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await _db.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task<List<Device>> GetAllAsync(CancellationToken ct)
        {
            return await _db.Devices
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public Task<Device?> GetByIdForUpdateAsync(long id, CancellationToken ct) =>
            _db.Devices.FirstOrDefaultAsync(d => d.Id == id, ct);
    }
}
