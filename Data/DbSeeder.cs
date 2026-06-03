using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Models;

namespace StreetSignalApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> hasher)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(new[]
            {
                new Category { Name = "Accumulated garbage",  Description = "Garbage accumulation in public spaces.", Icon = "trash",  Color = "#16A34A" },
                new Category { Name = "Broken streetlight",   Description = "Damaged or non-working streetlight.",     Icon = "bulb",   Color = "#F59E0B" },
                new Category { Name = "Pothole",              Description = "Damaged road surface.",                   Icon = "road",   Color = "#DC2626" },
                new Category { Name = "Water leak",           Description = "Public water leak.",                      Icon = "droplet",Color = "#0EA5E9" },
                new Category { Name = "Other",                Description = "Other issues.",                           Icon = "info",   Color = "#6B7280" }
            });
        }

        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Citizen))
        {
            var citizen = new User
            {
                FullName = "Citizen Demo",
                Email = "citizen@streetsignal.test",
                Phone = "8888-0000",
                Role = UserRole.Citizen
            };
            citizen.PasswordHash = hasher.HashPassword(citizen, "Password123!");
            db.Users.Add(citizen);
        }

        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Staff))
        {
            var staff = new User
            {
                FullName = "Staff Demo",
                Email = "staff@streetsignal.test",
                Phone = "8888-1111",
                Role = UserRole.Staff
            };
            staff.PasswordHash = hasher.HashPassword(staff, "Password123!");
            db.Users.Add(staff);
        }

        await db.SaveChangesAsync();
    }
}
