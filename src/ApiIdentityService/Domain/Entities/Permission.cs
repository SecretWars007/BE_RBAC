using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiIdentityService.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = default!; // ej: "inventory.read"
        public string Description { get; set; } = default!;
        public Guid ModuleId { get; set; }
        public Module Module { get; set; } = default!;
        public ICollection<RolePermission> RolePermissions { get; set; } =
            new List<RolePermission>();
    }
}
