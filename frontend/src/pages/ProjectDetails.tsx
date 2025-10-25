import React, { useEffect, useMemo, useState } from "react";
import api from "../api";
import { Link, useParams } from "react-router-dom";

type Task = {
  id: number;
  title: string;
  dueDate?: string | null;
  isCompleted: boolean;
  projectId: number;
  estimatedHours?: number | null;
};

type ProjectDetails = {
  id: number;
  title: string;
  description?: string | null;
  createdAt: string;
  tasks: Task[];
};

type ScheduleTimelineItem = {
  title: string;
  startOn: string;
  finishOn: string;
  estimatedHours: number;
  dueDate?: string | null;
  dependencies: string[];
};

type ScheduleResult = {
  projectId: number;
  title: string;
  recommendedOrder: string[];
  timeline: ScheduleTimelineItem[];
};

type PlannerState = Record<number, { estimatedHours: number; dependencies: number[] }>;

export default function ProjectDetails() {
  const { id } = useParams();
  const [project, setProject] = useState<ProjectDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [taskTitle, setTaskTitle] = useState("");
  const [taskDue, setTaskDue] = useState("");
  const [taskHours, setTaskHours] = useState("4");
  const [savingTask, setSavingTask] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [scheduleInfo, setScheduleInfo] = useState<ScheduleResult | null>(null);
  const [scheduleLoading, setScheduleLoading] = useState(false);
  const [scheduleError, setScheduleError] = useState<string | null>(null);
  const [planner, setPlanner] = useState<PlannerState>({});
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editValues, setEditValues] = useState({ title: "", dueDate: "", estimatedHours: "" });

  useEffect(() => {
    loadProject();
  }, [id]);

  useEffect(() => {
    if (!project) return;
    setPlanner((prev) => {
      const availableIds = new Set(project.tasks.map((t) => t.id));
      const next: PlannerState = {};

      for (const task of project.tasks) {
        const existing = prev[task.id];
        next[task.id] = {
          estimatedHours: existing?.estimatedHours ?? task.estimatedHours ?? 4,
          dependencies: (existing?.dependencies ?? []).filter((depId) => availableIds.has(depId)),
        };
      }

      return next;
    });
  }, [project?.id, project?.tasks]);

  const loadProject = async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    setScheduleError(null);
    try {
      const res = await api.get<ProjectDetails>(`/projects/${id}`);
      setProject(res.data);
      setScheduleInfo(null);
    } catch (err: any) {
      console.error(err);
      setError("Unable to load project details right now.");
    } finally {
      setLoading(false);
    }
  };

  const resetTaskForm = () => {
    setTaskTitle("");
    setTaskDue("");
    setTaskHours("4");
  };

  const handleCreateTask = async () => {
    if (!id || !taskTitle.trim()) {
      setError("Task title is required.");
      return;
    }

    const trimmedHours = taskHours.trim();
    const parsedHours = trimmedHours === "" ? null : Number(trimmedHours);
    if (parsedHours !== null && (!Number.isFinite(parsedHours) || parsedHours <= 0)) {
      setError("Estimated hours must be a positive number.");
      return;
    }

    setSavingTask(true);
    setError(null);
    try {
      await api.post(`/projects/${id}/tasks`, {
        title: taskTitle.trim(),
        dueDate: taskDue ? new Date(taskDue).toISOString() : null,
        estimatedHours: parsedHours !== null ? Math.round(parsedHours) : null,
      });
      resetTaskForm();
      await loadProject();
    } catch (err: any) {
      console.error(err);
      const msg = err?.response?.data?.error ?? "Could not add the task.";
      setError(msg);
    } finally {
      setSavingTask(false);
    }
  };

  const handleToggleTask = async (taskId: number) => {
    try {
      await api.patch(`/tasks/${taskId}/toggle`);
      await loadProject();
    } catch (err: any) {
      console.error(err);
      setError("Unable to update the task status. Please try again.");
    }
  };

  const handleRemoveTask = async (taskId: number) => {
    if (!window.confirm("Remove this task?")) return;
    try {
      await api.delete(`/tasks/${taskId}`);
      await loadProject();
    } catch (err: any) {
      console.error(err);
      setError("We couldn't delete that task.");
    }
  };

  const beginEdit = (task: Task) => {
    setEditingId(task.id);
    setEditValues({
      title: task.title,
      dueDate: task.dueDate ? task.dueDate.substring(0, 10) : "",
      estimatedHours: task.estimatedHours ? String(task.estimatedHours) : "",
    });
  };

  const saveEdit = async () => {
    if (!editingId) return;
    if (!editValues.title.trim()) {
      setError("Task title cannot be empty.");
      return;
    }

    const trimmedHours = editValues.estimatedHours.trim();
    const parsedHours = trimmedHours === "" ? null : Number(trimmedHours);
    if (parsedHours !== null && (!Number.isFinite(parsedHours) || parsedHours <= 0)) {
      setError("Estimated hours must be a positive number.");
      return;
    }

    try {
      await api.put(`/tasks/${editingId}`, {
        title: editValues.title.trim(),
        dueDate: editValues.dueDate ? new Date(editValues.dueDate).toISOString() : null,
        estimatedHours: parsedHours !== null ? Math.round(parsedHours) : null,
      });
      setEditingId(null);
      await loadProject();
    } catch (err: any) {
      console.error(err);
      setError(err?.response?.data?.error ?? "Unable to update the task.");
    }
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditValues({ title: "", dueDate: "", estimatedHours: "" });
  };

  const updatePlannerHours = (taskId: number, hours: number) => {
    const safeHours = Number.isFinite(hours) && hours > 0 ? Math.round(hours) : 1;
    setPlanner((prev) => ({
      ...prev,
      [taskId]: {
        estimatedHours: safeHours,
        dependencies: prev[taskId]?.dependencies ?? [],
      },
    }));
  };

  const toggleDependency = (taskId: number, dependencyId: number) => {
    setPlanner((prev) => {
      const current = prev[taskId] ?? { estimatedHours: project?.tasks.find((t) => t.id === taskId)?.estimatedHours ?? 4, dependencies: [] as number[] };
      const hasDependency = current.dependencies.includes(dependencyId);
      const nextDependencies = hasDependency
        ? current.dependencies.filter((id) => id !== dependencyId)
        : [...current.dependencies, dependencyId];

      return {
        ...prev,
        [taskId]: {
          estimatedHours: current.estimatedHours,
          dependencies: Array.from(new Set(nextDependencies.filter((depId) => depId !== taskId))),
        },
      };
    });
  };

  const clearDependencies = (taskId: number) => {
    setPlanner((prev) => ({
      ...prev,
      [taskId]: {
        estimatedHours: prev[taskId]?.estimatedHours ?? project?.tasks.find((t) => t.id === taskId)?.estimatedHours ?? 4,
        dependencies: [],
      },
    }));
  };

  const generateSchedule = async () => {
    if (!project) return;
    if (project.tasks.length === 0) {
      setScheduleError("Add at least one task to build a schedule.");
      return;
    }

    setScheduleLoading(true);
    setScheduleError(null);
    try {
      const payload = {
        tasks: project.tasks.map((task) => {
          const plan = planner[task.id] ?? { estimatedHours: task.estimatedHours ?? 4, dependencies: [] };
          const dependencyTitles = plan.dependencies
            .map((depId) => project.tasks.find((t) => t.id === depId)?.title)
            .filter((title): title is string => Boolean(title));

          return {
            title: task.title,
            estimatedHours: plan.estimatedHours,
            dueDate: task.dueDate,
            dependencies: dependencyTitles,
          };
        }),
      };

      const res = await api.post<ScheduleResult>(`/projects/${project.id}/schedule`, payload);
      setScheduleInfo(res.data);
    } catch (err: any) {
      console.error(err);
      const msg = err?.response?.data?.error ?? "We could not generate a schedule.";
      setScheduleError(msg);
    } finally {
      setScheduleLoading(false);
    }
  };

  const openTasks = useMemo(() => project?.tasks.filter((t) => !t.isCompleted).length ?? 0, [project?.tasks]);
  const completedTasks = useMemo(() => project?.tasks.filter((t) => t.isCompleted).length ?? 0, [project?.tasks]);
  const createdOn = useMemo(() => (project ? new Date(project.createdAt) : null), [project?.createdAt, project]);

  // formatting helpers for cleaner schedule UI
  const formatDay = (iso?: string | null) => {
    if (!iso) return "—";
    const d = new Date(iso);
    return d.toLocaleDateString(undefined, { day: "2-digit", month: "short", year: "numeric" });
  };

  const formatDateTime = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleString(undefined, {
      day: "2-digit",
      month: "short",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
    });
  };

  if (loading) {
    return <div className="card">Loading project...</div>;
  }

  if (!project) {
    return (
      <div className="card">
        <p>{error ?? "Project not found."}</p>
        <Link to="/dashboard" className="link mt-3 inline-block">Back to dashboard</Link>
      </div>
    );
  }

  return (
    <div className="detail-page space-y-6">
      <section className="surface-panel detail-hero">
        <div className="detail-hero__left">
          <div className="detail-crumb">
            <Link to="/dashboard" className="back-link">← Back to dashboard</Link>
            <span className="crumb-divider" aria-hidden="true">/</span>
          </div>
          <h1 className="detail-title">{project.title}</h1>
          {project.description && <p className="detail-subtitle">{project.description}</p>}
          <p className="detail-meta">Created {createdOn ? createdOn.toLocaleDateString() : "—"}</p>
        </div>
        <div className="detail-hero__right">
          <div className="detail-stat">
            <span>Open tasks</span>
            <strong>{openTasks}</strong>
          </div>
          <div className="detail-stat">
            <span>Completed</span>
            <strong>{completedTasks}</strong>
          </div>
          <button className="btn btn-primary detail-hero__cta" onClick={generateSchedule} disabled={scheduleLoading}>
            {scheduleLoading ? "Building..." : "Generate smart schedule"}
          </button>
        </div>
      </section>

      {error && <div className="form-alert form-alert--danger">{error}</div>}

      <div className="detail-layout">
        <section className="detail-column">
          <div className="surface-panel">
            <div className="section-heading">
              <div>
                <h2 className="section-title">Add task</h2>
                <p className="section-subtitle">Set intent, due date, and estimated hours.</p>
              </div>
            </div>
            <div className="mt-4 grid gap-3 sm:grid-cols-3">
              <label className="field-label sm:col-span-2">
                <span className="field-title">Task title</span>
                <input className="input" value={taskTitle} onChange={(e) => setTaskTitle(e.target.value)} placeholder="Kickoff meeting" />
              </label>
              <label className="field-label">
                <span className="field-title">Due date</span>
                <input className="input" type="date" value={taskDue} onChange={(e) => setTaskDue(e.target.value)} />
              </label>
              <label className="field-label">
                <span className="field-title">Est. hours</span>
                <input
                  className="input"
                  type="number"
                  min={1}
                  max={200}
                  value={taskHours}
                  onChange={(e) => setTaskHours(e.target.value)}
                />
              </label>
              <div className="sm:col-span-3 flex flex-col sm:flex-row sm:justify-end">
                <button className="btn btn-primary sm:w-auto" onClick={handleCreateTask} disabled={savingTask}>
                  {savingTask ? "Adding..." : "Add task"}
                </button>
              </div>
            </div>
          </div>

          <div className="surface-panel task-board">
            <div className="section-heading">
              <div>
                <h2 className="section-title">Tasks</h2>
                <p className="section-subtitle">Track progress and keep your backlog lean.</p>
              </div>
              <p className="section-meta">{openTasks} open · {project.tasks.length} total</p>
            </div>

            {project.tasks.length === 0 ? (
              <div className="empty-state">
                <h3>No tasks yet</h3>
                <p>Add your first task using the form above.</p>
              </div>
            ) : (
              <div className="task-list">
                {project.tasks.map((task) => {
                  const plan = planner[task.id];
                  const dependencies = plan?.dependencies ?? [];
                  return (
                    <article key={task.id} className={`task-card ${task.isCompleted ? "task-card--done" : ""}`}>
                      <div className="task-card__header">
                        <h3>{task.title}</h3>
                        <span className={`status-chip ${task.isCompleted ? "status-chip--done" : "status-chip--active"}`}>
                          {task.isCompleted ? "Completed" : "Open"}
                        </span>
                      </div>
                      <div className="task-card__meta">
                        <span>Due {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : "—"}</span>
                        <span>Est. {task.estimatedHours ?? "—"}h</span>
                        {dependencies.length > 0 && <span>{dependencies.length} {dependencies.length === 1 ? "dependency" : "dependencies"}</span>}
                      </div>
                      <div className="task-card__actions">
                        <button className="btn btn-ghost" onClick={() => handleToggleTask(task.id)}>
                          {task.isCompleted ? "Mark open" : "Mark done"}
                        </button>
                        <button className="btn btn-ghost" onClick={() => beginEdit(task)}>Edit</button>
                        <button className="btn btn-primary btn-primary--danger" onClick={() => handleRemoveTask(task.id)}>Delete</button>
                      </div>

                      {editingId === task.id && (
                        <div className="task-card__edit">
                          <input
                            className="input"
                            value={editValues.title}
                            onChange={(e) => setEditValues((prev) => ({ ...prev, title: e.target.value }))}
                          />
                          <div className="task-card__edit-grid">
                            <input
                              className="input"
                              type="date"
                              value={editValues.dueDate}
                              onChange={(e) => setEditValues((prev) => ({ ...prev, dueDate: e.target.value }))}
                            />
                            <input
                              className="input"
                              type="number"
                              min={1}
                              max={200}
                              placeholder="Est. hours"
                              value={editValues.estimatedHours}
                              onChange={(e) => setEditValues((prev) => ({ ...prev, estimatedHours: e.target.value }))}
                            />
                          </div>
                          <div className="task-card__edit-actions">
                            <button className="btn btn-primary" onClick={saveEdit}>Save changes</button>
                            <button className="btn btn-ghost" onClick={cancelEdit}>Cancel</button>
                          </div>
                        </div>
                      )}
                    </article>
                  );
                })}
              </div>
            )}
          </div>
        </section>

        <aside className="detail-column detail-column--right">
          <div className="surface-panel planner-panel">
            <div className="section-heading">
              <div>
                <h2 className="section-title">Schedule planner</h2>
                <p className="section-subtitle">Tune estimates and dependencies ahead of automation.</p>
              </div>
            </div>

            {project.tasks.length === 0 ? (
              <div className="empty-state">
                <h3>Nothing to plan yet</h3>
                <p>Add tasks to start using the planner.</p>
              </div>
            ) : (
              <div className="planner-list">
                {project.tasks.map((task) => {
                  const plan = planner[task.id];
                  return (
                    <div key={task.id} className="planner-item">
                      <p className="planner-title">{task.title}</p>
                      <label className="planner-field">
                        <span>Estimated hours</span>
                        <input
                          className="input"
                          type="number"
                          min={1}
                          max={200}
                          value={plan?.estimatedHours ?? 4}
                          onChange={(e) => updatePlannerHours(task.id, Number(e.target.value))}
                        />
                      </label>
                      <div className="planner-field planner-field--deps">
                        <div className="planner-field__header">
                          <span>Dependencies</span>
                          {(plan?.dependencies?.length ?? 0) > 0 && (
                            <button type="button" className="planner-clear" onClick={() => clearDependencies(task.id)}>
                              Clear all
                            </button>
                          )}
                        </div>
                        {project.tasks.filter((candidate) => candidate.id !== task.id).length === 0 ? (
                          <p className="planner-hint">Add another task to assign dependencies.</p>
                        ) : (
                          <div className="planner-checkboxes">
                            {project.tasks
                              .filter((candidate) => candidate.id !== task.id)
                              .map((candidate) => {
                                const selected = (plan?.dependencies ?? []).includes(candidate.id);
                                return (
                                  <label key={candidate.id} className="planner-checkbox">
                                    <input
                                      type="checkbox"
                                      checked={selected}
                                      onChange={() => toggleDependency(task.id, candidate.id)}
                                    />
                                    <span>{candidate.title}</span>
                                  </label>
                                );
                              })}
                          </div>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {scheduleError && <div className="form-alert form-alert--danger">{scheduleError}</div>}

          {scheduleInfo && (
            <div className="surface-panel timeline-panel">
              <div className="section-heading">
                <div>
                  <h2 className="section-title">Recommended order</h2>
                  <p className="section-subtitle">Just the sequence — no times.</p>
                </div>
              </div>
              <ul className="list-disc pl-6 space-y-2">
                {scheduleInfo.recommendedOrder.map((title) => (
                  <li key={title}>{title}</li>
                ))}
              </ul>
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}
