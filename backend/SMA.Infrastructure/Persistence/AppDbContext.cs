using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SMA.Domain.Entities;
using System.Linq.Expressions;

namespace SMA.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Event> Events => Set<Event>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ApplyGlobalSoftDeleteFilter(modelBuilder);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreationDate = utcNow;
                        entry.Entity.UpdateDate = null;
                        entry.Entity.DeletionDate = null;
                        break;

                    case EntityState.Modified:
                        entry.Property(x => x.CreationDate).IsModified = false;
                        entry.Entity.UpdateDate = utcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; // soft delete
                        entry.Entity.DeletionDate = utcNow;
                        entry.Entity.UpdateDate = utcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private static void ApplyGlobalSoftDeleteFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(BaseEntity.DeletionDate));
                    var nullConst = Expression.Constant(null, typeof(DateTime?));
                    var body = Expression.Equal(prop, nullConst);
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
    }
}
