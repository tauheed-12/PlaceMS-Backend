using IdentityService.Application.Interfaces;
using BCrypt.Net;

namespace IdentityService.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string HashPassword(string plainPassword)
        => BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);

    public bool VerifyPassword(string plainPassword, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
}