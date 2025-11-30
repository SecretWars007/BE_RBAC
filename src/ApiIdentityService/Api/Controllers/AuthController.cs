using System.Security.Claims;
using ApiIdentityService.Api.Dtos;
using ApiIdentityService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static ApiIdentityService.Application.Services.IAuthService;

namespace ApiIdentityService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _auth.RegisterAsync(
                new RegisterRequest(dto.UserName, dto.Email, dto.Password)
            );

            if (!string.IsNullOrEmpty(result.Token))
                return Ok(result);

            if (result.Message == "User already exists")
                return Conflict(result);

            return BadRequest(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _auth.LoginAsync(
                new LoginRequest(dto.UserName, dto.Password, dto.MfaCode)
            );

            if (result.MfaRequired && string.IsNullOrEmpty(result.Token))
                return Unauthorized(result);

            if (string.IsNullOrEmpty(result.Token))
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("mfa/enable")]
        [Authorize]
        public async Task<IActionResult> EnableMfa()
        {
            var userIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirst("sub")?.Value;

            if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var (secret, url, qrCodeBase64) = await _auth.EnableMfaAsync(userId);

            return Ok(
                new
                {
                    secret,
                    otpauthUrl = url,
                    qrCodeBase64,
                }
            );
        }
    }
}
