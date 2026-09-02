using DigitalArs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalArs.Infrastructure.Services;

public interface IAccountService
{
    Task<AccountDto?> GetAccountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<AccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken);
}
public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;

    public AccountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto?> GetAccountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new AccountDto(
                a.Id,
                a.Balance,
                a.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AccountDto(
                a.Id,
                a.Balance,
                a.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
