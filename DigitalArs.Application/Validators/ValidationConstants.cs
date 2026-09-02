namespace DigitalArs.Application.Validators;

/// <summary>
/// Constantes compartidas por los validadores. Los limites de negocio (por ejemplo,
/// el maximo por deposito de HU-15) idealmente vendran de configuracion (IOptions)
/// cuando se implemente el caso de uso; por ahora se centralizan aca.
/// </summary>
public static class ValidationConstants
{
    public const int PasswordMinLength = 8;
    public const int NameMaxLength = 50;
    public const int ConceptMaxLength = 200;

    /// <summary>Limite maximo por operacion de deposito (HU-15). Configurable a futuro.</summary>
    public const decimal MaxDepositPerOperation = 1_000_000m;

    /// <summary>Roles validos del sistema.</summary>
    public static readonly string[] ValidRoles = { "Admin", "User" };
}
