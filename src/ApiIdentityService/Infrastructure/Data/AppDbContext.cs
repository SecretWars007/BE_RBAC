using System;
using ApiIdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiIdentityService.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.HasIndex(u => u.UserName).IsUnique();
                b.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder
                .Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder
                .Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder
                .Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder
                .Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            modelBuilder.Entity<Module>().HasIndex(m => m.Key).IsUnique();

            modelBuilder.Entity<Permission>().HasIndex(p => p.Key).IsUnique();

            // ⬇⬇⬇ AQUI: seed de roles por defecto ⬇⬇⬇

            var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var userRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            modelBuilder
                .Entity<Role>()
                .HasData(new Role { Name = "Admin" }, new Role { Name = "User" });
        }
    }
}
