using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiIdentityService.Application.Security;
using ApiIdentityService.Domain.Entities;
using ApiIdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static ApiIdentityService.Application.Services.IAuthService;

namespace ApiIdentityService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IMfaService _mfa;

        public AuthService(AppDbContext db, IJwtTokenGenerator jwt, IMfaService mfa)
        {
            _db = db;
            _jwt = jwt;
            _mfa = mfa;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == request.UserName))
            {
                return new AuthResponse("", false, false, "User already exists");
            }

            var (hash, salt) = Security.PasswordHasher.HashPassword(request.Password);

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Por simplicidad, sin roles por defecto
            return new AuthResponse("", false, false, "Registered. Please login.");
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _db
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user is null)
            {
                return new AuthResponse(
                    Token: "",
                    MfaRequired: false,
                    MfaVerified: false,
                    Message: "Invalid credentials"
                );
            }

            var validPassword = PasswordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!validPassword)
            {
                return new AuthResponse(
                    Token: "",
                    MfaRequired: false,
                    MfaVerified: false,
                    Message: "Invalid credentials"
                );
            }

            // Password correcto → siempre generamos token
            bool mfaVerified = false;
            string message;

            if (user.IsMfaEnabled)
            {
                if (!string.IsNullOrWhiteSpace(request.MfaCode) && user.MfaSecret is not null)
                {
                    mfaVerified = _mfa.VerifyCode(user.MfaSecret, request.MfaCode);

                    message = mfaVerified
                        ? "OK (MFA verificado)"
                        : "OK (password válido, pero el código MFA es inválido)";
                }
                else
                {
                    message = "OK (password válido, MFA NO verificado porque no se envió código)";
                }
            }
            else
            {
                message = "OK (MFA no requerido)";
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var permissions = user
                .UserRoles.SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Key)
                .Distinct()
                .ToList();

            var token = _jwt.GenerateToken(user, roles, permissions);

            return new AuthResponse(
                Token: token,
                MfaRequired: user.IsMfaEnabled,
                MfaVerified: mfaVerified,
                Message: message
            );
        }

        public async Task<(string secret, string otpauthUrl, string qrCodeBase64)> EnableMfaAsync(
            Guid userId
        )
        {
            var user = await _db.Users.FindAsync(userId) ?? throw new Exception("User not found");

            var (secret, url, qrBase64) = _mfa.GenerateSecretForUser(user);
            user.MfaSecret = secret;
            user.IsMfaEnabled = true;

            await _db.SaveChangesAsync();
            return (secret, url, qrBase64);
        }
    }
}
