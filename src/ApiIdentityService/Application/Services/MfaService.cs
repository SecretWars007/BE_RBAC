using ApiIdentityService.Domain.Entities;
using OtpNet;
using QRCoder;

namespace ApiIdentityService.Application.Services
{
    public class MfaService : IMfaService
    {
        public (string secret, string otpauthUrl, string qrCodeBase64) GenerateSecretForUser(
            User user
        )
        {
            // 1) Generar clave secreta TOTP (20 bytes -> suficiente entropía)
            var key = KeyGeneration.GenerateRandomKey(20);
            var secret = Base32Encoding.ToString(key); // Base32 en mayúsculas, estándar

            // 2) Datos para otpauth://
            var issuer = "RbacHexagonalApp";
            var account = user.Email;

            var label = Uri.EscapeDataString($"{issuer}:{account}");
            var issuerParam = Uri.EscapeDataString(issuer);

            // IMPORTANTE: especificamos algoritmo, número de dígitos y periodo
            // Esto asegura que apps como Microsoft Authenticator usen exactamente
            // las mismas opciones que Otp.NET está usando.
            var otpauthUrl =
                $"otpauth://totp/{label}"
                + $"?secret={secret}"
                + $"&issuer={issuerParam}"
                + $"&algorithm=SHA1"
                + $"&digits=6"
                + $"&period=30";

            // 3) Generar QR (PNG) a partir del otpauthUrl
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(20); // tamaño del QR

            var qrCodeBase64 = Convert.ToBase64String(qrBytes);

            return (secret, otpauthUrl, qrCodeBase64);
        }

        public bool VerifyCode(string secret, string code)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
                return false;

            // Normalizar el código que viene del usuario:
            // quitar espacios, guiones, etc. por si los teclea así
            code = code.Trim().Replace(" ", string.Empty).Replace("-", string.Empty);

            // 4) Convertir el secreto Base32 a bytes
            var keyBytes = Base32Encoding.ToBytes(secret);

            // 5) Crear el TOTP configurado igual que en el otpauthUrl
            var totp = new Totp(secretKey: keyBytes, step: 30, mode: OtpHashMode.Sha1, totpSize: 6);

            // 6) Verificar el código con una ventana de tolerancia
            // previous=2, future=2 => acepta códigos de ±60s alrededor
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 2, future: 2));
        }
    }
}
