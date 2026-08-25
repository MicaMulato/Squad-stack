namespace Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key y propiedad de navegación con Role
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    // Propiedad de navegación bidireccional con Account
    public Account? Account { get; set; }
}
