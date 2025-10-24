
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.DTOs
{
    public record TaskCreateDto(
        [Required, StringLength(200, MinimumLength = 1)] string Title,
        DateTime? DueDate,
        [Range(1, 200, ErrorMessage = "Estimated hours should be between 1 and 200")] int? EstimatedHours);

    public record TaskUpdateDto(
        [Required, StringLength(200, MinimumLength = 1)] string Title,
        DateTime? DueDate,
        [Range(1, 200, ErrorMessage = "Estimated hours should be between 1 and 200")] int? EstimatedHours);

    public record TaskDto(int Id, string Title, DateTime? DueDate, bool IsCompleted, int ProjectId, int? EstimatedHours);
}
