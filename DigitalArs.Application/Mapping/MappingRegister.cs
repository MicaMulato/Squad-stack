using DigitalArs.Application.DTOs.Accounts;
using DigitalArs.Application.DTOs.Users;
using DigitalArs.Domain.Entities;
using Mapster;

namespace DigitalArs.Application.Mapping;

/// <summary>
/// Configuracion centralizada de mapeos Mapster (HU-08). Se registra al arranque
/// escaneando el ensamblado con TypeAdapterConfig.Scan, de modo que todo el mapeo
/// entidad -> DTO viva en un unico lugar.
///
/// Nota: las proyecciones para listados paginados e historial (User -> UserListItemResponse,
/// Transaction -> TransactionResponse) se resuelven mejor con .ProjectToType() sobre
/// IQueryable en la capa de servicio, para traducir a SQL y evitar N+1. Aca definimos
/// los mapeos base y las reglas que no son 1:1.
/// </summary>
public class MappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // User -> UserResponse: el rol sale de la navegacion Role.Name;
        // IsActive es el inverso de IsDeleted. No se expone PasswordHash.
        config.NewConfig<User, UserResponse>()
            .Map(dest => dest.Role, src => src.Role != null ? src.Role.Name : null)
            .Map(dest => dest.IsActive, src => !src.IsDeleted);

        // User -> UserListItemResponse: agrega el saldo desde la cuenta asociada.
        config.NewConfig<User, UserListItemResponse>()
            .Map(dest => dest.Role, src => src.Role != null ? src.Role.Name : null)
            .Map(dest => dest.IsActive, src => !src.IsDeleted)
            .Map(dest => dest.Balance, src => src.Account != null ? src.Account.Money : 0m);

        // Account -> AccountResponse: Money se expone como Balance.
        // CreatedAt mapea 1:1 (la entidad Account ahora tiene esa propiedad).
        config.NewConfig<Account, AccountResponse>()
            .Map(dest => dest.Balance, src => src.Money);
    }
}
