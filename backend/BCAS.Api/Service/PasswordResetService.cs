using BCAS.Api.Helper;
using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

public class PasswordResetService : IPasswordResetService
{
    private const int TokenExpiryMinutes = 30;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _resetRepository;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetRepository resetRepository,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _resetRepository = resetRepository;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return;
        }

        await IssueTokenAndSendEmailAsync(user, isInvite: false);
    }

    public async Task IssueInviteAsync(User newUser) => await IssueTokenAndSendEmailAsync(newUser, isInvite: true);

    private async Task IssueTokenAndSendEmailAsync(User user, bool isInvite)
    {
        var rawToken = SecureTokenGenerator.GenerateToken();
        var tokenHash = SecureTokenGenerator.HashToken(rawToken);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(TokenExpiryMinutes);

        await _resetRepository.CreateTokenAsync(user.Id, tokenHash, expiresAtUtc);

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
        var link = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

        var subject = isInvite ? "Set up your BCAS Admin Portal account" : "Reset your BCAS Admin Portal password";
        var intro = isInvite
            ? "An account has been created for you on the BCAS Admin Portal. Set your password to get started:"
            : "We received a request to reset your BCAS Admin Portal password. Click below to choose a new one:";

        var body = $"""
            <p>Hi {user.FullName},</p>
            <p>{intro}</p>
            <p><a href="{link}">{link}</a></p>
            <p>This link expires in {TokenExpiryMinutes} minutes and can only be used once.
            If you didn't expect this email, you can safely ignore it.</p>
            """;

        await _emailSender.SendAsync(user.Email, subject, body);
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(string token, string newPassword)
    {
        var tokenHash = SecureTokenGenerator.HashToken(token);
        var validToken = await _resetRepository.GetValidTokenAsync(tokenHash);
        if (validToken is null)
        {
            return ResetPasswordResult.InvalidOrExpiredToken;
        }

        var (hash, salt) = PasswordHasher.HashPassword(newPassword);
        await _userRepository.UpdatePasswordAsync(validToken.UserId, hash, salt);
        await _resetRepository.MarkUsedAsync(validToken.Id);
        await _resetRepository.InvalidateOutstandingTokensAsync(validToken.UserId);

        return ResetPasswordResult.Success;
    }
}
