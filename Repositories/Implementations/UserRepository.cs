using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Data;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Repositories.Implementations;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public async Task<IReadOnlyList<User>> GetActiveStaffAsync(CancellationToken ct = default) =>
        await _db.Users
            .Where(u => u.IsActive && u.Role == Common.Enums.UserRole.Staff)
            .ToListAsync(ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Email == email.ToLower(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        user.Email = user.Email.ToLower();
        await _db.Users.AddAsync(user, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
