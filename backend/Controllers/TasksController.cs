
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Models;
using ProjectManager.Api.Services;
using System.Security.Claims;

namespace ProjectManager.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _tasks;

        public TasksController(ITaskService tasks) => _tasks = tasks;

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost("projects/{projectId}/tasks")]
        public async Task<IActionResult> Create(int projectId, [FromBody] TaskCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var created = await _tasks.CreateAsync(projectId, CurrentUserId, dto);
                return CreatedAtAction(
                    nameof(Update),
                    new { taskId = created.Id },
                    MapTask(created));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("tasks/{taskId}")]
        public async Task<IActionResult> Update(int taskId, [FromBody] TaskUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var task = await _tasks.UpdateAsync(taskId, CurrentUserId, dto);
            if (task == null)
            {
                return NotFound();
            }

            return Ok(MapTask(task));
        }

        [HttpPatch("tasks/{taskId}/toggle")]
        public async Task<IActionResult> Toggle(int taskId)
        {
            var task = await _tasks.ToggleCompletionAsync(taskId, CurrentUserId);
            if (task == null)
            {
                return NotFound();
            }

            return Ok(MapTask(task));
        }

        [HttpDelete("tasks/{taskId}")]
        public async Task<IActionResult> Delete(int taskId)
        {
            var removed = await _tasks.DeleteAsync(taskId, CurrentUserId);
            if (!removed)
            {
                return NotFound();
            }

            return NoContent();
        }

        private static TaskDto MapTask(TaskItem task) =>
            new(task.Id, task.Title, task.DueDate, task.IsCompleted, task.ProjectId, task.EstimatedHours);
    }
}
