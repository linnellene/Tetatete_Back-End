using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain.Entities.Base;
using TetaBackend.Domain.Entities;

namespace TetaBackend.Domain;

public class DataContext: DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserInfoEntity> UserInfos { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateEntityTime(DateTimeOffset.UtcNow);

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateEntityTime(DateTimeOffset utcNow)
    {
        foreach (var entityEntry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = utcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = utcNow;
            }
        }
    }

}