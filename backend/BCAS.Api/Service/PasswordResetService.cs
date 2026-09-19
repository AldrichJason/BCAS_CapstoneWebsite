using BCAS.Api.Helper;
using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

public class PasswordResetService : IPasswordResetService
{
    private const int CodeExpiryMinutes = 30;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _resetRepository;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetRepository resetRepository,
        IEmailSender emailSender,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _userRepository = userRepository;
        _resetRepository = resetRepository;
        _emailSender = emailSender;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<string?> RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        return await IssueCodeAndSendEmailAsync(user, isInvite: false);
    }

    public Task<string?> IssueInviteAsync(User newUser) => IssueCodeAndSendEmailAsync(newUser, isInvite: true);

    private async Task<string?> IssueCodeAndSendEmailAsync(User user, bool isInvite)
    {
        var code = SecureTokenGenerator.GenerateNumericCode();
        var codeHash = SecureTokenGenerator.HashToken(code);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(CodeExpiryMinutes);

        await _resetRepository.CreateTokenAsync(user.Id, codeHash, expiresAtUtc);

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
        var resetPageUrl = $"{frontendBaseUrl}/reset-password";

        var subject = isInvite ? "Set up your BCAS Admin Portal account" : "Reset your BCAS Admin Portal password";
        var intro = isInvite
            ? "An account has been created for you on the BCAS Admin Portal. Use the code below to set your password:"
            : "We received a request to reset your BCAS Admin Portal password. Use the code below to choose a new one:";

        var body = $"""
            <p>Hi {user.FullName},</p>
            <p>{intro}</p>
            <p style="font-size: 28px; font-weight: bold; letter-spacing: 4px;">{code}</p>
            <p>Enter this code, along with your email, at <a href="{resetPageUrl}">{resetPageUrl}</a>.</p>
            <p>This code expires in {CodeExpiryMinutes} minutes and can only be used once.
            If you didn't expect this email, you can safely ignore it.</p>
            """;

        await _emailSender.SendAsync(user.Email, subject, body);

        // No mail server configured locally: nobody would otherwise receive this
        // code, so hand it back for the dev-only UI preview (never in Production,
        // and never when a real SMTP server is actually delivering the email).
        var smtpConfigured = !string.IsNullOrWhiteSpace(_configuration["Smtp:Host"]);
        return _environment.IsDevelopment() && !smtpConfigured ? code : null;
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(string email, string code, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return ResetPasswordResult.InvalidOrExpiredToken;
        }

        var codeHash = SecureTokenGenerator.HashToken(code);
        var validToken = await _resetRepository.GetValidTokenAsync(codeHash);
        if (validToken is null || validToken.UserId != user.Id)
        {
            return ResetPasswordResult.InvalidOrExpiredToken;
        }

        var (hash, salt) = PasswordHasher.HashPassword(newPassword);
        await _userRepository.UpdatePasswordAsync(user.Id, hash, salt);
        await _resetRepository.MarkUsedAsync(validToken.Id);
        await _resetRepository.InvalidateOutstandingTokensAsync(user.Id);

        return ResetPasswordResult.Success;
    }
}
