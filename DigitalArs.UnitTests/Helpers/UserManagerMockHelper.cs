using DigitalArs.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DigitalArs.UnitTests.Helpers;

public static class UserManagerMockHelper
{
    public static Mock<UserManager<User>> CreateMockUserManager(List<User> users)
    {
        var store = new Mock<IUserStore<User>>();
        var userManagerMock = new Mock<UserManager<User>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        var mockQueryable = users.BuildMockQueryable();
        userManagerMock.Setup(m => m.Users).Returns(mockQueryable);

        return userManagerMock;
    }
}
