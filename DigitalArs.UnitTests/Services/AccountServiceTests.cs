using System.Linq.Expressions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Application.Settings;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using DigitalArs.Infrastructure.Services;
using FluentAssertions;
using MapsterMapper;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DigitalArs.UnitTests.Services;

public class AccountServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBaseRepository<Account>> _accountRepoMock;
    private readonly Mock<IBaseRepository<Transaction>> _transactionRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IOptions<DepositSettings> _depositOptions;

    public AccountServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _accountRepoMock = new Mock<IBaseRepository<Account>>();
        _transactionRepoMock = new Mock<IBaseRepository<Transaction>>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Repository<Account>()).Returns(_accountRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Transaction>()).Returns(_transactionRepoMock.Object);

        _depositOptions = Options.Create(new DepositSettings
        {
            MaxAmountPerOperation = 100000m
        });
    }

    [Fact]
    public async Task DepositAsync_WithValidAmount_IncreasesBalanceAndCommitsTransaction()
    {
        // Arrange
        var userId = 1;
        var initialBalance = 5000m;
        var depositAmount = 2500m;
        var concept = "Ahorro mensual";

        var account = new Account
        {
            Id = 10,
            UserId = userId,
            Money = initialBalance,
            IsBlocked = false
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { account });

        Transaction? addedTransaction = null;
        _transactionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => addedTransaction = t)
            .Returns(Task.CompletedTask);

        var accountService = new AccountService(_unitOfWorkMock.Object, _mapperMock.Object, _depositOptions);

        // Act
        var result = await accountService.DepositAsync(userId, depositAmount, concept);

        // Assert
        result.Should().NotBeNull();
        result.NewBalance.Should().Be(7500m);
        account.Money.Should().Be(7500m);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _accountRepoMock.Verify(r => r.Update(account), Times.Once);
        _transactionRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        addedTransaction.Should().NotBeNull();
        addedTransaction!.AccountId.Should().Be(10);
        addedTransaction.Amount.Should().Be(depositAmount);
        addedTransaction.Type.Should().Be(TransactionType.Deposit);
        addedTransaction.Concept.Should().Be("Ahorro mensual");
    }

    [Fact]
    public async Task DepositAsync_WhenAmountExceedsMaxLimit_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var excessiveAmount = 200000m; // Límite es 100000m

        var accountService = new AccountService(_unitOfWorkMock.Object, _mapperMock.Object, _depositOptions);

        // Act
        var act = async () => await accountService.DepositAsync(userId, excessiveAmount);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*límite máximo permitido*");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task DepositAsync_WhenAccountNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = 99;
        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account>()); // No se encuentra cuenta

        var accountService = new AccountService(_unitOfWorkMock.Object, _mapperMock.Object, _depositOptions);

        // Act
        var act = async () => await accountService.DepositAsync(userId, 1000m);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*No se encontró una cuenta para el usuario con ID {userId}*");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task DepositAsync_WhenAccountIsBlocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var blockedAccount = new Account
        {
            Id = 15,
            UserId = userId,
            Money = 1000m,
            IsBlocked = true
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { blockedAccount });

        var accountService = new AccountService(_unitOfWorkMock.Object, _mapperMock.Object, _depositOptions);

        // Act
        var act = async () => await accountService.DepositAsync(userId, 500m);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*bloqueada*");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }
}
