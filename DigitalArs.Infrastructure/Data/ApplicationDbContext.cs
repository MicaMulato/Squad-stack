using DigitalArs.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configura las tablas de Identity (AspNetUsers, AspNetRoles, etc.)
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones IEntityTypeConfiguration<T> del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // === Seed de AspNetUserRoles ===
        // Identity resuelve [Authorize(Roles = "...")] leyendo la tabla intermedia
        // AspNetUserRoles, no la FK directa User.RoleId. Sin estas filas, el admin
        // seed recibiria 403 en endpoints protegidos por rol.
        // User 1 -> Admin (Role 1); Users 2 y 3 -> User (Role 2).
        modelBuilder.Entity<IdentityUserRole<int>>().HasData(
            new IdentityUserRole<int> { UserId = 1, RoleId = 1 },
            new IdentityUserRole<int> { UserId = 2, RoleId = 2 },
            new IdentityUserRole<int> { UserId = 3, RoleId = 2 },
            new IdentityUserRole<int> { UserId = 4, RoleId = 2 },
            new IdentityUserRole<int> { UserId = 5, RoleId = 2 },
            new IdentityUserRole<int> { UserId = 6, RoleId = 2 }
        );
    }
}
