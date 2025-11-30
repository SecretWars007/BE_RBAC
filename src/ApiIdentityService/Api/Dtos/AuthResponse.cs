using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiIdentityService.Api.Dtos
{
    public class AuthResponse
    {
        public string Token { get; set; } = "";

        // true si el usuario tiene MFA activado (IsMfaEnabled = true)
        public bool MfaRequired { get; set; }

        // true si en ESTE login se envió un código MFA válido
        public bool MfaVerified { get; set; }
        public string Message { get; set; } = "";
    }
}
