using System.ComponentModel.DataAnnotations;
using LumiaFoundation.Core.Domain.Exceptions;

namespace LumiaFoundation.Core.Validators;

public static class CommandValidator
{
    public static void Validate(object command)
    {
        var context = new ValidationContext(command);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(command, context, results, true))
        {
            throw new CommandValidationException(
                $"Falha na validação: {string.Join(", ", results.Select(r => r.ErrorMessage))}"
            );
        }
    }
}