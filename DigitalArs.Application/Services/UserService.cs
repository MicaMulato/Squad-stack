using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.Users;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserListItemResponse>> GetUsersAsync(UserFilterQuery query, CancellationToken cancellationToken = default)
    {
        var usersQuery = _userManager.Users
            .Include(u => u.Role)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim();
            usersQuery = usersQuery.Where(u =>
                u.FirstName.Contains(name) ||
                u.LastName.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = query.Email.Trim();
            usersQuery = usersQuery.Where(u => u.Email!.Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim();
            usersQuery = usersQuery.Where(u => u.Role != null && u.Role.Name == role);
        }

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(u => !u.IsDeleted == query.IsActive.Value);
        }

        var totalItems = await usersQuery.CountAsync(cancellationToken);

        var users = await usersQuery
            .OrderBy(u => u.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = _mapper.Map<List<UserListItemResponse>>(users);

        return new PagedResult<UserListItemResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null || user.IsDeleted)
        {
            return null;
        }

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            throw new ConflictException($"El email '{normalizedEmail}' ya se encuentra registrado.");
        }

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim();
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            throw new InvalidOperationException($"El rol '{roleName}' no existe.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                RoleId = role.Id,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync();
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al crear el usuario: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role.Name!);
            if (!roleResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync();
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al asignar el rol: {errors}");
            }

            // Generar CVU único de 22 dígitos
            var randomDigits = Random.Shared.Next(100000000, 999999999);
            var generatedCvu = $"0000003100010{randomDigits}";

            // Generar Alias base (ej. roberto.carlos.ars)
            var cleanFirst = request.FirstName.Trim().ToLowerInvariant().Replace(" ", "");
            var cleanLast = request.LastName.Trim().ToLowerInvariant().Replace(" ", "");
            var initialAlias = $"{cleanFirst}.{cleanLast}.ars";
            
            var aliasConflict = await _unitOfWork.Repository<Account>().Query()
                .AnyAsync(a => a.Alias == initialAlias, cancellationToken);
            if (aliasConflict)
            {
                initialAlias = $"{cleanFirst}.{cleanLast}.{Random.Shared.Next(100, 999)}.ars";
            }

            var account = new Account
            {
                UserId = user.Id,
                Money = request.InitialBalance,
                IsBlocked = false,
                Cvu = generatedCvu,
                Alias = initialAlias,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Account>().AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitAsync();

            user.Role = role;
            return _mapper.Map<UserResponse>(user);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null || user.IsDeleted)
        {
            return null;
        }

        var normalizedEmail = request.Email.Trim();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingWithEmail = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingWithEmail != null && existingWithEmail.Id != id)
            {
                throw new ConflictException($"El email '{normalizedEmail}' ya se encuentra registrado por otro usuario.");
            }
            user.Email = normalizedEmail;
            user.UserName = normalizedEmail;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim();
        if (user.Role == null || !string.Equals(user.Role.Name, roleName, StringComparison.OrdinalIgnoreCase))
        {
            var newRole = await _roleManager.FindByNameAsync(roleName);
            if (newRole == null)
            {
                throw new InvalidOperationException($"El rol '{roleName}' no existe.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, newRole.Name!);
            user.RoleId = newRole.Id;
            user.Role = newRole;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al actualizar el usuario: {errors}");
        }

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null || user.IsDeleted)
        {
            return false;
        }

        user.IsDeleted = true;
        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    // =========================================================================
    // HU-13: Ver y actualizar mis datos propios (/me)
    // =========================================================================

    public async Task<UserResponse?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || user.IsDeleted)
        {
            return null;
        }

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse?> UpdateMyProfileAsync(int userId, UpdateMyProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || user.IsDeleted)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                throw new InvalidOperationException("Debe indicar la contraseña actual para cambiarla.");
            }

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                throw new InvalidOperationException("La contraseña actual ingresada es incorrecta.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                var errors = string.Join("; ", changePasswordResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al cambiar la contraseña: {errors}");
            }
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al actualizar los datos: {errors}");
        }

        return _mapper.Map<UserResponse>(user);
    }
}
