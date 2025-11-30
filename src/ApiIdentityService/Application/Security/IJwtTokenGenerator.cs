using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiIdentityService.Domain.Entities;

namespace ApiIdentityService.Application.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    }
}
