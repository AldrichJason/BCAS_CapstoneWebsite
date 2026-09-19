namespace BCAS.Api.Repository.Interfaces;

public record ValidResetToken(int Id, int UserId);

public interface IPasswordResetRepository
{
    Task CreateTokenAsync(int userId, string tokenHash, DateTime expiresAtUtc);

    /// <summary>Returns the token row only if it exists, is unused and not expired.</summary>
    Task<ValidResetToken?> GetValidTokenAsync(string tokenHash);

    Task MarkUsedAsync(int tokenId);

    /// <summary>Marks every outstanding (unused) token for this user as used.</summary>
    Task InvalidateOutstandingTokensAsync(int userId);
}
