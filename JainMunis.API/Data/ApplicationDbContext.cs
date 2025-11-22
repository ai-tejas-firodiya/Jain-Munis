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
            entity.ToTable(t => t.HasCheckConstraint("CK_Schedules_ValidDateRange", "[EndDate] >= [StartDate]"));

            // Configure relationships
            entity.HasOne(s => s.Saint)
                .WithMany(s => s.Schedules)
                .HasForeignKey(s => s.SaintId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Location)
                .WithMany(l => l.Schedules)
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
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

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Create roles
        var adminRoleId = Guid.NewGuid().ToString();
        var userRoleId = Guid.NewGuid().ToString();

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new IdentityRole
            {
                Id = userRoleId,
                Name = "super_admin",
                NormalizedName = "SUPER_ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );

        // Create default super admin user
        var defaultAdminId = Guid.NewGuid().ToString();
        var hasher = new PasswordHasher<AdminUser>();
        var adminUser = new AdminUser
        {
            Id = defaultAdminId,
            UserName = "admin@jainmunis.app",
            NormalizedUserName = "ADMIN@JAINMUNIS.APP",
            Email = "admin@jainmunis.app",
            NormalizedEmail = "ADMIN@JAINMUNIS.APP",
            EmailConfirmed = true,
            PasswordHash = hasher.HashPassword(new AdminUser(), "Admin@123"),
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            Role = "super_admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<AdminUser>().HasData(adminUser);

        // Assign admin role to default user
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                UserId = defaultAdminId,
                RoleId = userRoleId,
            }
        );

        // Seed sample locations
        var mumbaiLocation = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Jain Temple, Dadar",
            Address = "123 S.V. Road, Dadar West",
            City = "Mumbai",
            State = "Maharashtra",
            Country = "India",
            CreatedAt = DateTime.UtcNow
        };

        var delhiLocation = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Jain Temple, Delhi",
            Address = "456 Ashok Road, Karol Bagh",
            City = "Delhi",
            State = "Delhi",
            Country = "India",
            CreatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Location>().HasData(mumbaiLocation, delhiLocation);

        // Seed sample saints
        var saint1 = new Saint
        {
            Id = Guid.NewGuid(),
            Name = "Acharya Mahashraman",
            Title = "Acharya",
            SpiritualLineage = "Terapanth tradition",
            Bio = "Current Acharya of the Terapanth tradition",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var saint2 = new Saint
        {
            Id = Guid.NewGuid(),
            Name = "Muni Sumermal",
            Title = "Muni",
            SpiritualLineage = "Terapanth tradition",
            Bio = "Disciple of Acharya Mahashraman",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Saint>().HasData(saint1, saint2);

        // Seed sample schedules
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        modelBuilder.Entity<Schedule>().HasData(
            new Schedule
            {
                Id = Guid.NewGuid(),
                SaintId = saint1.Id,
                LocationId = mumbaiLocation.Id,
                StartDate = today.AddDays(-1),
                EndDate = today.AddDays(5),
                Purpose = "Chaturmas",
                Notes = "Daily discourses at 6:00 PM",
                ContactPerson = "Ramesh Shah",
                ContactPhone = "+91 9876543210",
                CreatedBy = defaultAdminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Schedule
            {
                Id = Guid.NewGuid(),
                SaintId = saint2.Id,
                LocationId = delhiLocation.Id,
                StartDate = today.AddDays(10),
                EndDate = today.AddDays(15),
                Purpose = "Pravachan",
                Notes = "Special spiritual discourse series",
                ContactPerson = "Suresh Jain",
                ContactPhone = "+91 9876543211",
                CreatedBy = defaultAdminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}