using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Infrastructure;

public class NotEmptyGuidAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is Guid guid && guid == Guid.Empty)
        {
            return new ValidationResult("The field must not be an empty GUID.");
        }
        return ValidationResult.Success;
    }
}
