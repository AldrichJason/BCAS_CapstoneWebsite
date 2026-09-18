using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Repository.Interfaces;

namespace BCAS.Api.Middleware;

/// <summary>
/// Runs after authentication on every request carrying a valid JWT signature.
/// Rejects the request if the token was revoked at logout or the account was
/// deactivated after the token was issued (BW-11, BW-15).
/// </summary>
public class ActiveSessionMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var revoked = !string.IsNullOrEmpty(jti) && await userRepository.IsTokenRevokedAsync(jti);
            var stillActive = int.TryParse(userIdClaim, out var userId)
                && await userRepository.GetByIdAsync(userId) is { IsActive: true };

            if (revoked || !stillActive)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    new ErrorResponseDto("Session is no longer valid. Please log in again.")));
                return;
            }
        }

        await _next(context);
    }
}
