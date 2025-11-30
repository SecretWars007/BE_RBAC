using ApiIdentityService.Domain.Entities;

namespace ApiIdentityService.Application.Services
{
    public interface IMfaService
    {
        (string secret, string otpauthUrl, string qrCodeBase64) GenerateSecretForUser(User user);
        bool VerifyCode(string secret, string code);
    }
}
