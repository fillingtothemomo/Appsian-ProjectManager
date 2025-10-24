
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.Models
{
    public class User
    {
        public int Id { get; set; }
    [Required, MaxLength(120)]
    public string DisplayName { get; set; } = default!;

    [Required, MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = default!;
        [Required]
        public byte[] PasswordHash { get; set; } = default!;
        [Required]
        public byte[] PasswordSalt { get; set; } = default!;
        public List<Project> Projects { get; set; } = new();
    }
}
