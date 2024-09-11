using BC = BCrypt.Net.BCrypt;

namespace TetaBackend.User.Utilities;

public static class PasswordHasher
{
    private const int SaltRounds = 10;
    
    public static string HashPassword(string password)
    {
        return BC.HashPassword(password, SaltRounds);
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        return BC.Verify(password, passwordHash);
    }
}