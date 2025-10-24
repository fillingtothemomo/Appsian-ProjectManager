using ProjectManager.Api.DTOs;
using ProjectManager.Api.Models;

namespace ProjectManager.Api.Services
{
    public interface IScheduleService
    {
        ScheduleResultDto BuildSchedule(Project project, ScheduleRequestDto? request);
    }

    public class ScheduleService : IScheduleService
    {
        public ScheduleResultDto BuildSchedule(Project project, ScheduleRequestDto? request)
        {
            var normalizedTasks = NormalizeTasks(project, request);
            var ordered = ResolveOrder(normalizedTasks);
            var timeline = BuildTimeline(ordered);

            var orderTitles = ordered.Select(t => t.Title).ToList();
            return new ScheduleResultDto(orderTitles, timeline);
        }

        private static IReadOnlyCollection<TaskDefinition> NormalizeTasks(Project project, ScheduleRequestDto? request)
        {
            if (request?.Tasks is { Count: > 0 })
            {
                return request.Tasks
                    .Select(t => new TaskDefinition(
                        t.Title.Trim(),
                        Math.Max(t.EstimatedHours, 1),
                        t.DueDate,
                        t.Dependencies?.Select(d => d.Trim()).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>()))
                    .ToList();
            }

            return project.Tasks
                .Select(t => new TaskDefinition(
                    t.Title,
                    Math.Max(t.EstimatedHours ?? 4, 1),
                    t.DueDate,
                    Array.Empty<string>()))
                .ToList();
        }

        private static List<TaskDefinition> ResolveOrder(IReadOnlyCollection<TaskDefinition> tasks)
        {
            if (tasks.Count == 0)
            {
                throw new InvalidOperationException("No tasks available to build a schedule.");
            }

            var lookup = new Dictionary<string, TaskDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var task in tasks)
            {
                if (!lookup.TryAdd(task.Title, task))
                {
                    throw new InvalidOperationException($"Duplicate task title detected: {task.Title}");
                }
            }

            var indegree = tasks.ToDictionary(t => t.Title, t => 0, StringComparer.OrdinalIgnoreCase);
            var adjacency = tasks.ToDictionary(t => t.Title, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);

            foreach (var task in tasks)
            {
                foreach (var dependency in task.Dependencies)
                {
                    if (!lookup.ContainsKey(dependency))
                    {
                        throw new InvalidOperationException($"Unknown dependency '{dependency}' referenced by task '{task.Title}'.");
                    }

                    indegree[task.Title]++;
                    adjacency[dependency].Add(task.Title);
                }
            }

            var ready = tasks.Where(t => indegree[t.Title] == 0).ToList();
            var ordered = new List<TaskDefinition>(tasks.Count);
            var comparer = new TaskSequenceComparer();

            while (ready.Count > 0)
            {
                ready.Sort(comparer);
                var current = ready[0];
                ready.RemoveAt(0);
                ordered.Add(current);

                foreach (var dependent in adjacency[current.Title])
                {
                    indegree[dependent]--;
                    if (indegree[dependent] == 0)
                    {
                        ready.Add(lookup[dependent]);
                    }
                }
            }

            if (ordered.Count != tasks.Count)
            {
                throw new InvalidOperationException("Circular dependency detected while building the schedule.");
            }

            return ordered;
        }

        private static IReadOnlyList<ScheduledTaskDto> BuildTimeline(IReadOnlyList<TaskDefinition> ordered)
        {
            var cursor = DateTime.UtcNow.Date.AddHours(9); // start at 9 AM today
            var timeline = new List<ScheduledTaskDto>(ordered.Count);

            foreach (var task in ordered)
            {
                var hours = Math.Max(task.EstimatedHours, 1);
                var start = cursor;
                var finish = start.AddHours(hours);
                cursor = finish;

                timeline.Add(new ScheduledTaskDto(
                    task.Title,
                    start,
                    finish,
                    hours,
                    task.DueDate,
                    task.Dependencies));
            }

            return timeline;
        }

        private sealed record TaskDefinition(string Title, int EstimatedHours, DateTime? DueDate, IReadOnlyCollection<string> Dependencies);

        private sealed class TaskSequenceComparer : IComparer<TaskDefinition>
        {
            public int Compare(TaskDefinition? x, TaskDefinition? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return 1;
                if (y == null) return -1;

                var dueX = x.DueDate ?? DateTime.MaxValue;
                var dueY = y.DueDate ?? DateTime.MaxValue;
                var dueCompare = dueX.CompareTo(dueY);
                if (dueCompare != 0) return dueCompare;

                var hoursCompare = x.EstimatedHours.CompareTo(y.EstimatedHours);
                if (hoursCompare != 0) return hoursCompare;

                return string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
