// ElectivesApp.Tests/TemplateEngineTests.cs
using Xunit;
using ElectivesApp.Core;
using System.Collections.Generic;

namespace ElectivesApp.Tests;

public class TemplateEngineTests
{
    [Fact]
    public void RenderString_ShouldReplacePlaceholders()
    {
        // Arrange
        var template = "Привіт, {{name}}!";
        var model = new Dictionary<string, object?> { ["name"] = "Олексій" };

        // Act
        var result = TemplateEngine.RenderString(template, model);

        // Assert
        Assert.Equal("Привіт, Олексій!", result);
    }

    [Fact]
    public void ProcessIf_ShouldShowContent_WhenConditionIsTrue()
    {
        var template = "{{#if show}}Видно{{/if}}";
        var model = new Dictionary<string, object?> { ["show"] = true };

        var result = TemplateEngine.RenderString(template, model);

        Assert.Equal("Видно", result);
    }
}