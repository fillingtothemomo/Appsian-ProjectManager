
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManager.Api.Models
{
    public class Project
    {
        public int Id { get; set; }
        [Required, MinLength(3), MaxLength(100)]
        public string Title { get; set; } = default!;
        [MaxLength(500)]
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Owner
        public int UserId { get; set; }
        public User? User { get; set; }

        public List<TaskItem> Tasks { get; set; } = new();
    }
}
