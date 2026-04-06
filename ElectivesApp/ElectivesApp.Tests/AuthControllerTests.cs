// ElectivesApp.Tests/AuthControllerTests.cs
using ElectivesApp.Core;
using ElectivesApp.Core.Controllers;
using ElectivesApp.DAO;
using ElectivesApp.Infrastructure;
using ElectivesApp.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ElectivesApp.Tests;

public class AuthControllerTests
{
    [Fact]
    public void PostLogin_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        var mockUserDao = new Mock<IUserDao>();
        mockUserDao.Setup(d => d.GetByUsername("unknown")).Returns((User?)null);

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        var sessions = new SessionStore();
        var config = new AppConfig();

        var controller = new AuthController(mockUserDao.Object, sessions, config, mockLoggerFactory.Object);

        // Тут зазвичай потрібно імітувати HttpContext, що складно з HttpListener, 
        // тому рекомендується виносити бізнес-логіку перевірки в окремий сервіс.
    }
}