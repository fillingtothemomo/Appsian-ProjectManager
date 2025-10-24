
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Services;
using System.Security.Claims;

namespace ProjectManager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/projects/{projectId}/schedule")]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly IProjectService _projects;
        private readonly IScheduleService _scheduler;

        public ScheduleController(IProjectService projects, IScheduleService scheduler)
        {
            _projects = projects;
            _scheduler = scheduler;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> Generate(int projectId, [FromBody] ScheduleRequestDto? request)
        {
            var project = await _projects.GetById(projectId, CurrentUserId);
            if (project == null)
            {
                return NotFound();
            }

            try
            {
                var result = _scheduler.BuildSchedule(project, request);
                return Ok(new
                {
                    projectId = project.Id,
                    project.Title,
                    result.RecommendedOrder,
                    timeline = result.Timeline
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
