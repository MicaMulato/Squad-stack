using DigitalArs.Application.DTOs.Auth;
using DigitalArs.Application.Interfaces;
using DigitalArs.Application.Services;
using DigitalArs.Domain.Entities;
using DigitalArs.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DigitalArs.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;

    public AuthServiceTests()
    {
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@digitalars.com",
            IsDeleted = false,
            Role = new Role { Id = 1, Name = "User" }
        };

        var usersList = new List<User> { user };
        var userManagerMock = UserManagerMockHelper.CreateMockUserManager(usersList);

        userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, "Password123!"))
            .ReturnsAsync(true);

        userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var expectedExpiresAt = DateTime.UtcNow.AddHours(2);
        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateToken(user, "User"))
            .Returns(("jwt-mock-token-abc-123", expectedExpiresAt));

        var authService = new AuthService(userManagerMock.Object, _jwtTokenGeneratorMock.Object);
        var loginRequest = new LoginRequest
        {
            Email = "test@digitalars.com",
            Password = "Password123!"
        };

        // Act
        var result = await authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-mock-token-abc-123");
        result.UserId.Should().Be(1);
        result.Email.Should().Be("test@digitalars.com");
        result.Role.Should().Be("User");
        result.ExpiresAt.Should().Be(expectedExpiresAt);
    }

    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@digitalars.com",
            IsDeleted = false,
            Role = new Role { Id = 1, Name = "User" }
        };

        var usersList = new List<User> { user };
        var userManagerMock = UserManagerMockHelper.CreateMockUserManager(usersList);

        userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, "WrongPassword!"))
            .ReturnsAsync(false);

        var authService = new AuthService(userManagerMock.Object, _jwtTokenGeneratorMock.Object);
        var loginRequest = new LoginRequest
        {
            Email = "test@digitalars.com",
            Password = "WrongPassword!"
        };

        // Act
        var result = await authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
        _jwtTokenGeneratorMock.Verify(j => j.GenerateToken(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveOrDeletedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var inactiveUser = new User
        {
            Id = 2,
            Email = "inactive@digitalars.com",
            IsDeleted = true, // Usuario dado de baja / inactivo
            Role = new Role { Id = 1, Name = "User" }
        };

        var usersList = new List<User> { inactiveUser };
        var userManagerMock = UserManagerMockHelper.CreateMockUserManager(usersList);

        userManagerMock
            .Setup(m => m.CheckPasswordAsync(inactiveUser, "Password123!"))
            .ReturnsAsync(true);

        var authService = new AuthService(userManagerMock.Object, _jwtTokenGeneratorMock.Object);
        var loginRequest = new LoginRequest
        {
            Email = "inactive@digitalars.com",
            Password = "Password123!"
        };

        // Act
        var act = async () => await authService.LoginAsync(loginRequest);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inactivo*");

        _jwtTokenGeneratorMock.Verify(j => j.GenerateToken(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        var usersList = new List<User>();
        var userManagerMock = UserManagerMockHelper.CreateMockUserManager(usersList);

        var authService = new AuthService(userManagerMock.Object, _jwtTokenGeneratorMock.Object);
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@digitalars.com",
            Password = "Password123!"
        };

        // Act
        var result = await authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }
}
