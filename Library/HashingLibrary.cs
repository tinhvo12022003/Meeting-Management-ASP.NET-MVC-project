using Microsoft.AspNetCore.Identity;

namespace MeetingManagement.Library;

public class HashingLibrary
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string plainPassword)
    {
        return _hasher.HashPassword(new object(), plainPassword);
    }

    public bool VerifyPassword(string inputPassword, string storedHash)
    {
        var result = _hasher.VerifyHashedPassword(
            new object(),
            storedHash,
            inputPassword);

        return result == PasswordVerificationResult.Success;
    }

    public string HashRefreshToken(string refreshToken)
    {
        return _hasher.HashPassword(new object(), refreshToken);
    }

    public bool VerifyRefreshToken(string inputToken, string storedHash)
    {
        var result = _hasher.VerifyHashedPassword(
            new object(),
            storedHash,
            inputToken);

        return result == PasswordVerificationResult.Success;
    }

}