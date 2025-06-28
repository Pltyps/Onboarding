using System.ComponentModel.DataAnnotations;

namespace MOAI.API.Validation
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_extensions.Contains(ext))
                {
                    return new ValidationResult($"Only the following file types are allowed: {string.Join(", ", _extensions)}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
