using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiIdentityService.Domain.Entities
{
    public class Module
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = default!; // ej: "inventory"
        public string Name { get; set; } = default!;
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
