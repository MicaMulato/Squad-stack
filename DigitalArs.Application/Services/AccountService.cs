using DigitalArs.Application.DTOs.Accounts;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using MapsterMapper;

namespace DigitalArs.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AccountService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // HU-14: Obtener datos de la cuenta por el Id del usuario autenticado
    public async Task<AccountResponse?> GetAccountByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _unitOfWork.Repository<Account>()
            .FindAsync(a => a.UserId == userId);

        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            return null;
        }

        return _mapper.Map<AccountResponse>(account);
    }

    // HU-14: Obtener cuenta por su Id (opcional / admin)
    public async Task<AccountResponse?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Repository<Account>().GetByIdAsync(accountId);
        if (account == null)
        {
            return null;
        }

        return _mapper.Map<AccountResponse>(account);
    }
}
