using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.Users;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using Mapster;
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
        IQueryable<User> queryable;

        if (query.IsActive.HasValue)
        {
            if (query.IsActive.Value)
            {
                queryable = _userManager.Users
                    .Include(u => u.Role)
                    .Include(u => u.Account)
                    .Where(u => !u.IsDeleted);
            }
            else
            {
                queryable = _userManager.Users
                    .IgnoreQueryFilters()
                    .Include(u => u.Role)
                    .Include(u => u.Account)
                    .Where(u => u.IsDeleted);
            }
        }
        else
        {
            queryable = _userManager.Users
                .IgnoreQueryFilters()
                .Include(u => u.Role)
                .Include(u => u.Account);
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var nameTrimmed = query.Name.Trim();
            queryable = queryable.Where(u => u.FirstName.Contains(nameTrimmed) || u.LastName.Contains(nameTrimmed));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var emailTrimmed = query.Email.Trim();
            queryable = queryable.Where(u => u.Email != null && u.Email.Contains(emailTrimmed));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var roleTrimmed = query.Role.Trim();
            queryable = queryable.Where(u => u.Role != null && u.Role.Name == roleTrimmed);
        }

        var totalItems = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectToType<UserListItemResponse>()
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemResponse>(items, query.Page, query.PageSize, totalItems);
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
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

            var account = new Account
            {
                UserId = user.Id,
                Money = request.InitialBalance,
                IsBlocked = false,
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
}
