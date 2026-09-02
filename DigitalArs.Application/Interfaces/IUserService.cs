using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.Users;

namespace DigitalArs.Application.Interfaces;

public interface IUserService
{
    // HU-12: CRUD Administrativo de Usuarios
    Task<PagedResult<UserListItemResponse>> GetUsersAsync(UserFilterQuery query, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);

    // HU-13: Ver y actualizar mis datos propios (/me)
    Task<UserResponse?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateMyProfileAsync(int userId, UpdateMyProfileRequest request, CancellationToken cancellationToken = default);
}
