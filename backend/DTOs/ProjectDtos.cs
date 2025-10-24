
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.DTOs
{
    public record ProjectCreateDto(
        [Required, StringLength(100, MinimumLength = 3)] string Title,
        [StringLength(500)] string? Description);

    public record ProjectUpdateDto(
        [Required, StringLength(100, MinimumLength = 3)] string Title,
        [StringLength(500)] string? Description);

    public record ProjectSummaryDto(int Id, string Title, string? Description, DateTime CreatedAt);

    public record ProjectDetailsDto(int Id, string Title, string? Description, DateTime CreatedAt, IReadOnlyCollection<TaskDto> Tasks);
}
