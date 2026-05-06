using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Auth
{
    public class OAuthLoginRequest
    {
        [Required]
        public string Provider { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ProviderKey { get; set; } = string.Empty;
    }
}
