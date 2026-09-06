
using System.ComponentModel.DataAnnotations;
using LumiaFoundation.AspNetCore.Commons.Exceptions;

namespace LumiaFoundation.AspNetCore.Commons.Validators;

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
