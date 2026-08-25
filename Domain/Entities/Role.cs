namespace Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Propiedad de navegación bidireccional
    public ICollection<User> Users { get; set; } = new List<User>();
}
