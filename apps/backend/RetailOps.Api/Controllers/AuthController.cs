using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Application.UseCases;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthenticateUserUseCase authenticate,
    VerifyManagerPinUseCase verifyManagerPin,
    IUserRepository users,
    IJwtTokenService jwt) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AuthenticateUserInDto request, CancellationToken ct)
    {
        var result = await authenticate.Execute(request);
        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register([FromBody] RegisterUserInDto request) =>
        StatusCode(StatusCodes.Status501NotImplemented, new { error = "Trial registration is handled in EP-002 Platform." });

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = jwt.GetUserIdFromPrincipal(User);
        if (userId is null)
            return Unauthorized();

        var user = await users.FindByIdAsync(userId.Value, ct);
        if (user is null)
            return NotFound();

        return Ok(new CurrentUserOutDto(
            user.Id,
            user.LegacyUserId,
            user.TenantId.Value,
            user.Name,
            user.Email?.Value,
            user.Level.ToString(),
            user.Grants.Select(g => g.PermissionKey.Value).ToList()));
    }

    [HttpPost("verify-manager-pin")]
    [Authorize]
    public async Task<IActionResult> VerifyManagerPin([FromBody] VerifyManagerPinInDto request, CancellationToken ct)
    {
        var userId = jwt.GetUserIdFromPrincipal(User);
        if (userId is null)
            return Unauthorized();

        var result = await verifyManagerPin.Execute(new VerifyManagerPinCommand(userId.Value, request.Pin));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { verified = true });
    }
}
