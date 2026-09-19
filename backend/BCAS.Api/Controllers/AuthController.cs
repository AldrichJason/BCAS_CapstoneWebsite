using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(IAuthService authService, IPasswordResetService passwordResetService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
    }

    // BW-10: Role-based login with JWT authentication.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.LoginAsync(request);
        if (result is null)
        {
            return Unauthorized(new ErrorResponseDto("Invalid email or password."));
        }

        return Ok(result);
    }

    // BW-12: Session check endpoint with role and department scope.
    [HttpGet("session")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Session()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ErrorResponseDto("Invalid session token."));
        }

        var user = await _authService.GetActiveUserAsync(userId);
        if (user is null)
        {
            return Unauthorized(new ErrorResponseDto("Session is no longer valid."));
        }

        return Ok(user);
    }

    // BW-11: Logout and session termination.
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (!string.IsNullOrEmpty(jti) && long.TryParse(expClaim, out var expUnix))
        {
            var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            await _authService.LogoutAsync(jti, expiresAtUtc);
        }

        return NoContent();
    }

    // BW-13: Forgot password. Always returns the same response so the
    // endpoint can't be used to enumerate which emails have accounts.
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _passwordResetService.RequestPasswordResetAsync(request.Email);

        return Ok(new MessageResponseDto("If an account exists for that email, a reset link has been sent."));
    }

    // BW-13: Reset password using the single-use token from the emailed link.
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _passwordResetService.ResetPasswordAsync(request.Token, request.NewPassword);
        if (result == ResetPasswordResult.InvalidOrExpiredToken)
        {
            return BadRequest(new ErrorResponseDto("This reset link is invalid or has expired. Please request a new one."));
        }

        return Ok(new MessageResponseDto("Your password has been reset. You can now sign in."));
    }
}
