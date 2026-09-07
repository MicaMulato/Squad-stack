using DigitalArs.Application.DTOs.FixedTermDeposits;
using DigitalArs.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace DigitalArs.UnitTests.Services;

public class FixedTermDepositServiceTests
{
    [Theory]
    [InlineData(30, 19.0)]
    [InlineData(45, 19.0)]
    [InlineData(60, 22.0)]
    [InlineData(90, 25.0)]
    [InlineData(180, 30.0)]
    [InlineData(365, 35.0)]
    public void GetInterestRateForDuration_ReturnsExpectedRates(int days, decimal expectedRate)
    {
        // Act
        var rate = FixedTermDepositService.GetInterestRateForDuration(days);

        // Assert
        rate.Should().Be(expectedRate);
    }

    [Fact]
    public void Simulate_CalculatesInterestAndFinalAmountAccurately()
    {
        // Arrange
        var request = new SimulateFixedTermDepositRequest
        {
            Amount = 100000m,
            DurationDays = 30
        };

        // Act
        var simulation = new FixedTermDepositService(null!, null!).Simulate(request);

        // Assert
        simulation.Should().NotBeNull();
        simulation.Amount.Should().Be(100000m);
        simulation.InterestRate.Should().Be(19.0m);
        simulation.DurationDays.Should().Be(30);

        // Interés = 100000 * (19 / 100 / 365) * 30 = 1561.64
        var expectedInterest = Math.Round(100000m * (19.0m / 100m / 365m) * 30, 2);
        simulation.InterestEarned.Should().Be(expectedInterest);
        simulation.FinalAmount.Should().Be(100000m + expectedInterest);
    }
}
