using Microsoft.AspNetCore.Identity;

namespace DigitalArs.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key y propiedad de navegacion con Role (no se usa RoleId propio, se maneja via IdentityUserRole)
    // Pero mantenemos la navegacion directa para simplificar consultas
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    // Propiedad de navegacion bidireccional con Account
    public Account? Account { get; set; }
}
