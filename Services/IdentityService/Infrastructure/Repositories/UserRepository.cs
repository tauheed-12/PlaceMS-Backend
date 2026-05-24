using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
        => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByEmailWithTokensAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token), ct);

    public async Task<User?> GetByVerificationTokenAsync(string token, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token, ct);

    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public void Update(User user)
        => _context.Users.Update(user);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}