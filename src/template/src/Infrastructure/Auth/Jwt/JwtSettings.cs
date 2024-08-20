using System.ComponentModel.DataAnnotations;

namespace Genocs.Microservice.Template.Infrastructure.Auth.Jwt;

/// <summary>
/// The JwtSettings class is used to configure the JWT settings.
/// This class uses the IValidatableObject interface to validate the configuration.
/// </summary>
public class JwtSettings : IValidatableObject
{
    public string Key { get; set; } = string.Empty;

    public int TokenExpirationInMinutes { get; set; }

    public int RefreshTokenExpirationInDays { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Key))
        {
            yield return new ValidationResult("No Key defined in JwtSettings config", new[] { nameof(Key) });
        }
    }
}