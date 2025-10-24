
using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.Data;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Models;

namespace ProjectManager.Api.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetByUser(int userId);
        Task<Project?> GetById(int id, int userId);
        Task<Project> Create(Project project);
        Task Delete(int id, int userId);
    }

    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;
        public ProjectService(AppDbContext db) => _db = db;

        public async Task<Project> Create(Project project)
        {
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task Delete(int id, int userId)
        {
            var p = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (p == null) return;
            _db.Projects.Remove(p);
            await _db.SaveChangesAsync();
        }

        public Task<Project?> GetById(int id, int userId) =>
            _db.Projects
                .Include(p => p.Tasks)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        public async Task<IEnumerable<Project>> GetByUser(int userId) =>
            await _db.Projects
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
    }
}
