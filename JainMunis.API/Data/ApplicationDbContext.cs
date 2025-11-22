using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using JainMunis.API.Models.Entities;

namespace JainMunis.API.Data;

public class ApplicationDbContext : IdentityDbContext<AdminUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Saint> Saints { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }
    public DbSet<NotificationQueue> NotificationQueues { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Saint entity
        modelBuilder.Entity<Saint>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.PhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Configure Location entity
        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100).HasDefaultValue("India");
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Schedule entity
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.Property(e => e.Purpose).HasMaxLength(255);
            entity.Property(e => e.ContactPerson).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Add constraint for valid date range
            entity.HasCheckConstraint("CK_Schedules_ValidDateRange", "[EndDate] >= [StartDate]");

            // Configure relationships
            entity.HasOne(s => s.Saint)
                .WithMany(s => s.Schedules)
                .HasForeignKey(s => s.SaintId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Location)
                .WithMany(l => l.Schedules)
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Creator)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure AdminUser entity
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("admin");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Configure NotificationSubscription entity
        modelBuilder.Entity<NotificationSubscription>(entity =>
        {
            entity.Property(e => e.UserIdentifier).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.EmailEnabled).HasDefaultValue(true);
            entity.Property(e => e.WhatsAppEnabled).HasDefaultValue(false);
            entity.Property(e => e.PushEnabled).HasDefaultValue(false);
        });

        // Configure NotificationQueue entity
        modelBuilder.Entity<NotificationQueue>(entity =>
        {
            entity.Property(e => e.UserIdentifier).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Channel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.RetryCount).HasDefaultValue(0);

            entity.HasOne(n => n.NotificationSubscription)
                .WithMany(ns => ns.QueuedNotifications)
                .HasForeignKey(n => n.NotificationSubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ActivityLog entity
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(a => a.AdminUser)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(a => a.AdminUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Create unique index for UserIdentifier in NotificationSubscription
        modelBuilder.Entity<NotificationSubscription>()
            .HasIndex(ns => ns.UserIdentifier)
            .IsUnique();
    }
}