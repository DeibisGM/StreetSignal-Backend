using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Data;
using StreetSignalApi.Models;

namespace StreetSignalApi.IntegrationTests;

public class StreetSignalWebAppFactory : WebApplicationFactory<Program>
{
    public Guid CitizenId { get; private set; }
    public Guid StaffId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid InactiveCategoryId { get; private set; }

    public const string Password = "Password123!";
    public const string CitizenEmail = "citizen@test.com";
    public const string StaffEmail   = "staff@test.com";

    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "StreetSignal-Test",
                ["Jwt:Audience"] = "StreetSignal-Test",
                ["Jwt:SigningKey"] = "test-signing-key-with-32+characters-1234567890",
                ["Jwt:ExpiresInSeconds"] = "3600"
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor is not null) services.Remove(dbDescriptor);

            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            // Build the provider and seed test data
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            db.Database.EnsureCreated();
            Seed(db, hasher);
        });
    }

    private void Seed(AppDbContext db, IPasswordHasher<User> hasher)
    {
        if (db.Users.Any()) return;

        var citizen = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Citizen Test",
            Email = CitizenEmail,
            Role = UserRole.Citizen,
            IsActive = true
        };
        citizen.PasswordHash = hasher.HashPassword(citizen, Password);
        CitizenId = citizen.Id;

        var staff = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Staff Test",
            Email = StaffEmail,
            Role = UserRole.Staff,
            IsActive = true
        };
        staff.PasswordHash = hasher.HashPassword(staff, Password);
        StaffId = staff.Id;

        var active = new Category { Id = Guid.NewGuid(), Name = "Pothole", IsActive = true };
        var inactive = new Category { Id = Guid.NewGuid(), Name = "Legacy", IsActive = false };
        CategoryId = active.Id;
        InactiveCategoryId = inactive.Id;

        db.Users.AddRange(citizen, staff);
        db.Categories.AddRange(active, inactive);
        db.SaveChanges();
    }
}
