using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.DTOs
{
    public record ScheduleTaskInputDto(
        [Required, StringLength(200, MinimumLength = 1)] string Title,
        [Range(1, 200, ErrorMessage = "Estimated hours should be between 1 and 200")] int EstimatedHours,
        DateTime? DueDate,
        IReadOnlyCollection<string>? Dependencies
    );

    public record ScheduleRequestDto(IReadOnlyCollection<ScheduleTaskInputDto>? Tasks);

    public record ScheduledTaskDto(string Title, DateTime StartOn, DateTime FinishOn, int EstimatedHours, DateTime? DueDate, IReadOnlyCollection<string> Dependencies);

    public record ScheduleResultDto(IReadOnlyList<string> RecommendedOrder, IReadOnlyList<ScheduledTaskDto> Timeline);
}
