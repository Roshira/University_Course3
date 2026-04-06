// ElectivesApp.Tests/RouterTests.cs
using Xunit;
using ElectivesApp.Core;
using System.Threading.Tasks;

namespace ElectivesApp.Tests;

public class RouterTests
{
    [Fact]
    public void Match_ShouldExtractParameters_FromUrl()
    {
        // Arrange
        var router = new Router();
        router.Get("/teacher/courses/{id}/edit", ctx => Task.CompletedTask);

        // Act
        var (route, parameters) = router.Match("GET", "/teacher/courses/15/edit");

        // Assert
        Assert.NotNull(route);
        Assert.Equal("15", parameters["id"]);
    }
}