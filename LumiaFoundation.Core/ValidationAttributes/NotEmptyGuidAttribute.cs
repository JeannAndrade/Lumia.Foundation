using System.ComponentModel.DataAnnotations;

namespace LumiaFoundation.Core.ValidationAttributes;

public class NotEmptyGuidAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is Guid guid && guid == Guid.Empty)
        {
            return new ValidationResult($"{validationContext.DisplayName} não pode ser Guid.Empty.");
        }

        return ValidationResult.Success!;
    }
}
