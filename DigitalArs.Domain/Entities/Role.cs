using Microsoft.AspNetCore.Identity;

namespace DigitalArs.Domain.Entities;

public class Role : IdentityRole<int>
{
    public string? Description { get; set; }

    // Propiedad de navegacion bidireccional
    public ICollection<User> Users { get; set; } = new List<User>();
}
