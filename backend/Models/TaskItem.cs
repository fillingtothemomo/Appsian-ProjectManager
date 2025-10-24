
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManager.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        [Required, MinLength(1), MaxLength(200)]
        public string Title { get; set; } = default!;
    public DateTime? DueDate { get; set; }
    [Range(1, 200)]
    public int? EstimatedHours { get; set; }
        public bool IsCompleted { get; set; } = false;

        // Parent project
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}
