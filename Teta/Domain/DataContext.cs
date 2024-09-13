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
    public DbSet<GenderEntity> Genders { get; set; }
    public DbSet<LanguageEntity> Languages { get; set; }
    public DbSet<LocationEntity> Locations { get; set; }
    public DbSet<UserInfoLanguageEntity> UserInfoLanguages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserInfoEntity>()
            .HasOne(ui => ui.PlaceOfBirth)
            .WithMany(l => l.UserInfoBirthPlaces)
            .HasForeignKey(ui => ui.PlaceOfBirthId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<UserInfoEntity>()
            .HasOne(ui => ui.Location)
            .WithMany(l => l.UserInfoLocations)
            .HasForeignKey(ui => ui.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<UserInfoLanguageEntity>()
            .HasKey(ur => new { ur.UserInfoId, ur.LanguageId });

        modelBuilder.Entity<UserInfoLanguageEntity>()
            .HasOne(ur => ur.Language)
            .WithMany(u => u.UserInfoLanguages)
            .HasForeignKey(ur => ur.LanguageId);

        modelBuilder.Entity<UserInfoLanguageEntity>()
            .HasOne(ur => ur.UserInfo)
            .WithMany(r => r.UserInfoLanguages)
            .HasForeignKey(ur => ur.UserInfoId);
        
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