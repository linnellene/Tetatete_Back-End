using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain.Entities.Base;
using TetaBackend.Domain.Entities;
using TetaBackend.Domain.Entities.CategoryInfo;

namespace TetaBackend.Domain;

public class DataContext: DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    
    public DbSet<UserEntity> Users { get; set; }
    
    public DbSet<UserInfoEntity> UserInfos { get; set; }
    
    public DbSet<ImageEntity> Images { get; set; }
    
    public DbSet<GenderEntity> Genders { get; set; }
    
    public DbSet<LanguageEntity> Languages { get; set; }
    
    public DbSet<LocationEntity> Locations { get; set; }
    
    public DbSet<UserInfoLanguageEntity> UserInfoLanguages { get; set; }
    
    public DbSet<FriendsCategoryInfoEntity> FriendsCategoryInfos { get; set; }
    
    public DbSet<LoveCategoryInfoEntity> LoveCategoryInfos { get; set; }
    
    public DbSet<WorkCategoryInfoEntity> WorkCategoryInfos { get; set; }
    
    public DbSet<MatchEntity> Matches { get; set; }
    
    public DbSet<ChatEntity> Chats { get; set; }
    
    public DbSet<MessageEntity> Messages { get; set; }
    
    public DbSet<NotificationEntity> Notifications { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasOne(u => u.UserInfo)
            .WithOne()
            .HasForeignKey<UserEntity>(u => u.UserInfoId)
            .IsRequired(false);
        
        modelBuilder.Entity<UserInfoEntity>()
            .HasOne(f => f.User)
            .WithOne()
            .HasForeignKey<UserInfoEntity>(f => f.UserId)
            .IsRequired();
        
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
        
        modelBuilder.Entity<UserEntity>()
            .HasOne(u => u.FriendsCategoryInfo)
            .WithOne()
            .HasForeignKey<UserEntity>(u => u.FriendsCategoryInfoId)
            .IsRequired(false);
        
        modelBuilder.Entity<FriendsCategoryInfoEntity>()
            .HasOne(f => f.User)
            .WithOne()
            .HasForeignKey<FriendsCategoryInfoEntity>(f => f.UserId)
            .IsRequired();
        
        modelBuilder.Entity<UserEntity>()
            .HasOne(u => u.LoveCategoryInfo)
            .WithOne()
            .HasForeignKey<UserEntity>(u => u.LoveCategoryInfoId)
            .IsRequired(false);
        
        modelBuilder.Entity<LoveCategoryInfoEntity>()
            .HasOne(f => f.User)
            .WithOne()
            .HasForeignKey<LoveCategoryInfoEntity>(f => f.UserId)
            .IsRequired();
        
        modelBuilder.Entity<UserEntity>()
            .HasOne(u => u.WorkCategoryInfo)
            .WithOne()
            .HasForeignKey<UserEntity>(u => u.WorkCategoryInfoId)
            .IsRequired(false);
        
        modelBuilder.Entity<WorkCategoryInfoEntity>()
            .HasOne(f => f.User)
            .WithOne()
            .HasForeignKey<WorkCategoryInfoEntity>(f => f.UserId)
            .IsRequired();
        
        modelBuilder.Entity<MatchEntity>()
            .HasKey(um => new { um.InitiatorId, um.ReceiverId });

        modelBuilder.Entity<MatchEntity>()
            .HasOne(um => um.Initiator)
            .WithMany(u => u.InitiatedMatches)
            .HasForeignKey(um => um.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchEntity>()
            .HasOne(um => um.Receiver)
            .WithMany(u => u.ReceivedMatches)
            .HasForeignKey(um => um.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatEntity>()
            .HasOne(um => um.UserA)
            .WithMany(u => u.ChatsA)
            .HasForeignKey(um => um.UserAId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatEntity>()
            .HasOne(um => um.UserB)
            .WithMany(u => u.ChatsB)
            .HasForeignKey(um => um.UserBId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<MessageEntity>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Restrict);
    
        modelBuilder.Entity<MessageEntity>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        
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