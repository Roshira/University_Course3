// ElectivesApp.Tests/SessionStoreTests.cs
using Xunit;
using ElectivesApp.Core;
using System;
using System.Threading;

namespace ElectivesApp.Tests;

public class SessionStoreTests
{
    [Fact]
    public void Get_ShouldReturnNull_WhenSessionIsExpired()
    {
        // Arrange
        var store = new SessionStore(timeoutMinutes: -1); // Вже прострочена
        var session = store.Create(1, "student", "testuser");

        // Act
        var retrieved = store.Get(session.Id);

        // Assert
        Assert.Null(retrieved);
    }
}