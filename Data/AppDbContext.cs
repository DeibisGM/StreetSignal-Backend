using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Models;

namespace StreetSignalApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportUpdate> ReportUpdates => Set<ReportUpdate>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // User
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.Email).IsRequired().HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
        });

        // Category
        b.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Icon).HasMaxLength(50);
            e.Property(x => x.Color).HasMaxLength(20);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Report
        b.Entity<Report>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(150);
            e.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            e.Property(x => x.Status).HasConversion<string>().IsRequired();
            e.Property(x => x.Priority).HasConversion<string>();
            e.Property(x => x.Address).HasMaxLength(255);
            e.Property(x => x.ImageUrl).HasMaxLength(1000);

            e.HasOne(x => x.Category)
                .WithMany(c => c.Reports)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.CreatedBy)
                .WithMany(u => u.Reports)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.CreatedById);
            e.HasIndex(x => x.CreatedAt);
        });

        // ReportUpdate
        b.Entity<ReportUpdate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<string>().IsRequired();
            e.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            e.Property(x => x.IsOfficial).IsRequired();
            e.Property(x => x.OldStatus).HasConversion<string>();
            e.Property(x => x.NewStatus).HasConversion<string>();

            e.HasOne(x => x.Report)
                .WithMany(r => r.Updates)
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.ReportId, x.CreatedAt });
        });

        // Notification
        b.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Message).IsRequired().HasMaxLength(1000);

            e.HasOne(x => x.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Report)
                .WithMany()
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => new { x.UserId, x.IsRead });
        });

        // DeviceToken
        b.Entity<DeviceToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).IsRequired().HasMaxLength(500);
            e.Property(x => x.Platform).HasConversion<string>().IsRequired();

            e.HasOne(x => x.User)
                .WithMany(u => u.DeviceTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.Token).IsUnique();
        });
    }
}
