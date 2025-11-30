using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiIdentityService.Application.Services
{
    public interface IAuthService
    {
        public record RegisterRequest(string UserName, string Email, string Password);

        public record LoginRequest(string UserName, string Password, string? MfaCode);

        public record AuthResponse(
            string Token,
            bool MfaRequired,
            bool MfaVerified,
            string? Message
        );

        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<(string secret, string otpauthUrl, string qrCodeBase64)> EnableMfaAsync(Guid userId);
    }
}
