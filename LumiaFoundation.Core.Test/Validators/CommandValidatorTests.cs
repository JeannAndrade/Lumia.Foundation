using System.ComponentModel.DataAnnotations;
using LumiaFoundation.Core.Domain.Exceptions;
using LumiaFoundation.Core.Validators;

namespace LumiaFoundation.Core.Test.Validators;

public class CommandValidatorTests
{
    private sealed class ValidatedCommand
    {
        [Required]
        public string? Name { get; init; }
    }

    [Fact]
    public void Validate_WhenCommandIsValid_DoesNotThrow()
    {
        // Arrange
        var command = new ValidatedCommand { Name = "ok" };

        // Act
        var exception = Record.Exception(() => CommandValidator.Validate(command));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenCommandIsInvalid_ThrowsCommandValidationException()
    {
        // Arrange
        var command = new ValidatedCommand();

        // Act
        var exception = Assert.Throws<CommandValidationException>(() => CommandValidator.Validate(command));

        // Assert
        Assert.Contains("Falha na validação", exception.Message);
        Assert.Contains("Name", exception.Message);
    }
}
