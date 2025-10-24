using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.Data;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Models;

namespace ProjectManager.Api.Services
{
    public interface ITaskService
    {
        Task<TaskItem> CreateAsync(int projectId, int userId, TaskCreateDto dto);
        Task<TaskItem?> UpdateAsync(int taskId, int userId, TaskUpdateDto dto);
        Task<TaskItem?> ToggleCompletionAsync(int taskId, int userId);
        Task<bool> DeleteAsync(int taskId, int userId);
    }

    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TaskItem> CreateAsync(int projectId, int userId, TaskCreateDto dto)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }

            var title = dto.Title.Trim();

            var task = new TaskItem
            {
                Title = title,
                DueDate = dto.DueDate,
                EstimatedHours = dto.EstimatedHours,
                ProjectId = projectId
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> UpdateAsync(int taskId, int userId, TaskUpdateDto dto)
        {
            var task = await _db.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Project!.UserId == userId);

            if (task == null)
            {
                return null;
            }

            task.Title = dto.Title.Trim();
            task.DueDate = dto.DueDate;
            task.EstimatedHours = dto.EstimatedHours;

            await _db.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> ToggleCompletionAsync(int taskId, int userId)
        {
            var task = await _db.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Project!.UserId == userId);

            if (task == null)
            {
                return null;
            }

            task.IsCompleted = !task.IsCompleted;
            await _db.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteAsync(int taskId, int userId)
        {
            var task = await _db.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Project!.UserId == userId);

            if (task == null)
            {
                return false;
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
