using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiIdentityService.Api.Dtos
{
    public class LoginDto
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? MfaCode { get; set; }
    }
}
