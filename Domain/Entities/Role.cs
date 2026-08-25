namespace Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Propiedad de navegación bidireccional
    public ICollection<User> Users { get; set; } = new List<User>();
}
