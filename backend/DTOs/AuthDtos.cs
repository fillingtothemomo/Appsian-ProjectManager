
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.DTOs
{
    public record RegisterDto(
        [Required, StringLength(120, MinimumLength = 2)] string Name,
        [Required, EmailAddress, StringLength(200)] string Email,
        [Required, StringLength(100, MinimumLength = 6)] string Password);

    public record LoginDto(
        [Required, StringLength(120, MinimumLength = 2)] string Name,
        [Required, EmailAddress, StringLength(200)] string Email,
        [Required, StringLength(100, MinimumLength = 6)] string Password);

    public record AuthResultDto(string Token, string Name, string Email);
}
