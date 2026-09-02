namespace DigitalArs.Application.Settings;

/// <summary>
/// Configuración para operaciones de depósito.
/// Se bindea desde la sección "DepositSettings" de appsettings.json
/// mediante IOptions&lt;DepositSettings&gt;.
/// </summary>
public class DepositSettings
{
    public decimal MaxAmountPerOperation { get; set; }
}
