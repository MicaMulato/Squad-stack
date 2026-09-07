using System.Linq.Expressions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using DigitalArs.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DigitalArs.UnitTests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBaseRepository<Account>> _accountRepoMock;
    private readonly Mock<IBaseRepository<Transaction>> _transactionRepoMock;

    public TransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _accountRepoMock = new Mock<IBaseRepository<Account>>();
        _transactionRepoMock = new Mock<IBaseRepository<Transaction>>();

        _unitOfWorkMock.Setup(u => u.Repository<Account>()).Returns(_accountRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Transaction>()).Returns(_transactionRepoMock.Object);
    }

    [Fact]
    public async Task TransferAsync_WhenValid_ExecutesSuccessfullyAndCommits()
    {
        // Arrange
        var sourceUserId = 1;
        var destinationAccountId = 2;
        var transferAmount = 3000m;
        var concept = "Alquiler";

        var sourceAccount = new Account
        {
            Id = 1,
            UserId = sourceUserId,
            Money = 10000m,
            IsBlocked = false
        };

        var destAccount = new Account
        {
            Id = destinationAccountId,
            UserId = 2,
            Money = 1500m,
            IsBlocked = false
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { sourceAccount });

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(destinationAccountId))
            .ReturnsAsync(destAccount);

        var addedTransactions = new List<Transaction>();
        _transactionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => addedTransactions.Add(t))
            .Returns(Task.CompletedTask);

        var service = new TransactionService(_unitOfWorkMock.Object);

        // Act
        var result = await service.TransferAsync(sourceUserId, destinationAccountId, transferAmount, concept);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(transferAmount);
        result.NewBalance.Should().Be(7000m);

        sourceAccount.Money.Should().Be(7000m);
        destAccount.Money.Should().Be(4500m);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _accountRepoMock.Verify(r => r.Update(sourceAccount), Times.Once);
        _accountRepoMock.Verify(r => r.Update(destAccount), Times.Once);
        _transactionRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        addedTransactions.Should().HaveCount(2);
        addedTransactions.Should().ContainSingle(t => t.Type == TransactionType.TransferOut && t.AccountId == 1 && t.ToAccountId == 2);
        addedTransactions.Should().ContainSingle(t => t.Type == TransactionType.TransferIn && t.AccountId == 2 && t.ToAccountId == 1);
    }

    [Fact]
    public async Task TransferAsync_WhenInsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var sourceUserId = 1;
        var destinationAccountId = 2;
        var transferAmount = 5000m;

        var sourceAccount = new Account
        {
            Id = 1,
            UserId = sourceUserId,
            Money = 1000m, // Solo tiene 1000, quiere transferir 5000
            IsBlocked = false
        };

        var destAccount = new Account
        {
            Id = destinationAccountId,
            UserId = 2,
            Money = 500m,
            IsBlocked = false
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { sourceAccount });

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(destinationAccountId))
            .ReturnsAsync(destAccount);

        var service = new TransactionService(_unitOfWorkMock.Object);

        // Act
        var act = async () => await service.TransferAsync(sourceUserId, destinationAccountId, transferAmount);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Saldo insuficiente*");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_WhenDestinationAccountDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sourceUserId = 1;
        var destinationAccountId = 999;

        var sourceAccount = new Account
        {
            Id = 1,
            UserId = sourceUserId,
            Money = 5000m,
            IsBlocked = false
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { sourceAccount });

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(destinationAccountId))
            .ReturnsAsync((Account?)null);

        var service = new TransactionService(_unitOfWorkMock.Object);

        // Act
        var act = async () => await service.TransferAsync(sourceUserId, destinationAccountId, 1000m);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*La cuenta destino con ID {destinationAccountId} no existe*");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_WhenSelfTransfer_ThrowsArgumentException()
    {
        // Arrange
        var sourceUserId = 1;
        var sameAccountId = 1;

        var sourceAccount = new Account
        {
            Id = sameAccountId,
            UserId = sourceUserId,
            Money = 5000m,
            IsBlocked = false
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { sourceAccount });

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(sameAccountId))
            .ReturnsAsync(sourceAccount);

        var service = new TransactionService(_unitOfWorkMock.Object);

        // Act
        var act = async () => await service.TransferAsync(sourceUserId, sameAccountId, 1000m);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*No se puede transferir dinero a tu propia cuenta*");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_WhenDestinationAccountIsBlocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var sourceUserId = 1;
        var destinationAccountId = 2;

        var sourceAccount = new Account
        {
            Id = 1,
            UserId = sourceUserId,
            Money = 5000m,
            IsBlocked = false
        };

        var blockedDestAccount = new Account
        {
            Id = destinationAccountId,
            UserId = 2,
            Money = 500m,
            IsBlocked = true
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { sourceAccount });

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(destinationAccountId))
            .ReturnsAsync(blockedDestAccount);

        var service = new TransactionService(_unitOfWorkMock.Object);

        // Act
        var act = async () => await service.TransferAsync(sourceUserId, destinationAccountId, 1000m);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*bloqueada y no puede recibir transferencias*");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_WhenCommitFails_ThrowsException()
    {
        // Arrange
        var sourceUserId = 1;
        var destinationAccountId = 2;

        var sourceAccount = new Account
        {
            Id = 1,
            UserId = sourceUserId,
            Money = 5000m,
            IsBlocked = false
        };

        var destAccount = new Account
        {
            Id = destinationAccountId,
            UserId = 2,
            Money = 500m,
            IsBlocked = false
        };

        _accountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account> { sourceAccount });

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(destinationAccountId))
            .ReturnsAsync(destAccount);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .ThrowsAsync(new InvalidOperationException("Fallo en la base de datos al confirmar transacción"));

        var service = new TransactionService(_unitOfWorkMock.Object);

        // Act
        var act = async () => await service.TransferAsync(sourceUserId, destinationAccountId, 1000m);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Fallo en la base de datos al confirmar transacción");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
