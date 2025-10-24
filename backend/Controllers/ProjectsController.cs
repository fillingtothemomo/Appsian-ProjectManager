
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Models;
using ProjectManager.Api.Services;

namespace ProjectManager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projects;
        public ProjectsController(IProjectService projects) => _projects = projects;

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projects.GetByUser(CurrentUserId);
            var dto = projects
                .Select(p => new ProjectSummaryDto(p.Id, p.Title, p.Description, p.CreatedAt))
                .ToList();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var project = new Project
            {
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                UserId = CurrentUserId
            };

            var created = await _projects.Create(project);
            var response = new ProjectSummaryDto(created.Id, created.Title, created.Description, created.CreatedAt);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projects.GetById(id, CurrentUserId);
            if (project == null)
            {
                return NotFound();
            }

            var tasks = project.Tasks
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
                .Select(t => new TaskDto(t.Id, t.Title, t.DueDate, t.IsCompleted, t.ProjectId, t.EstimatedHours))
                .ToList();

            var dto = new ProjectDetailsDto(project.Id, project.Title, project.Description, project.CreatedAt, tasks);
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _projects.Delete(id, CurrentUserId);
            return NoContent();
        }
    }
}
