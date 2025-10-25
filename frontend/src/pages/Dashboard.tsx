import React, { useEffect, useMemo, useRef, useState } from "react";
import api from "../api";
import { Link } from "react-router-dom";

type ProjectSummary = {
  id: number;
  title: string;
  description?: string | null;
  createdAt: string;
};

export default function Dashboard() {
  const [projects, setProjects] = useState<ProjectSummary[]>([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading] = useState(false);
  const [isBooting, setIsBooting] = useState(true);
  const [creationError, setCreationError] = useState<string | null>(null);
  const [bannerMessage, setBannerMessage] = useState<string | null>(null);
  const [profileName, setProfileName] = useState(() => localStorage.getItem("profileName") ?? "");
  const titleFieldRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    fetchProjects();
  }, []);

  useEffect(() => {
    const handleStorage = () => setProfileName(localStorage.getItem("profileName") ?? "");
    window.addEventListener("storage", handleStorage);
    return () => window.removeEventListener("storage", handleStorage);
  }, []);

  const fetchProjects = async () => {
    setCreationError(null);
    setBannerMessage(null);
    try {
      const res = await api.get<ProjectSummary>("/projects");
      setProjects(res.data as unknown as ProjectSummary[]);
    } catch (err: any) {
      console.error(err);
      setBannerMessage("We couldn't load your projects. Try refreshing.");
    } finally {
      setIsBooting(false);
    }
  };

  const createProject = async () => {
    if (!title.trim()) {
      setCreationError("Project title is required.");
      return;
    }

    setLoading(true);
    setCreationError(null);
    setBannerMessage(null);
    try {
      await api.post("/projects", { title: title.trim(), description: description.trim() || null });
      setTitle("");
      setDescription("");
      await fetchProjects();
    } catch (err: any) {
      console.error(err);
      setCreationError(err?.response?.data?.error ?? "Unable to create project right now.");
    } finally {
      setLoading(false);
    }
  };

  const deleteProject = async (id: number) => {
    if (!window.confirm("Delete this project? This cannot be undone.")) return;
    setBannerMessage(null);
    try {
      await api.delete(`/projects/${id}`);
      await fetchProjects();
    } catch (err: any) {
      console.error(err);
      setBannerMessage("Failed to delete the project. Please retry.");
    }
  };

  const totalProjects = projects.length;
  const firstName = useMemo(() => {
    const trimmed = profileName.trim();
    if (!trimmed) return "";
    return trimmed.split(" ")[0];
  }, [profileName]);

  return (
    <div className="workspace-page">
      <header className="workspace-header">
        <div className="workspace-header__top">
          <div>
            <div className="workspace-pills">
              <span className="workspace-pill">Dashboard</span>
              {firstName && <span className="workspace-pill workspace-pill--ghost">Welcome, {firstName}</span>}
            </div>
            <h1 className="workspace-title">Projects overview</h1>
          </div>
          <div className="workspace-actions">
            <button className="btn btn-ghost" onClick={() => titleFieldRef.current?.focus()}>Capture idea</button>
            <button className="btn btn-primary" onClick={createProject} disabled={loading}>
              {loading ? "Creating..." : "Create project"}
            </button>
          </div>
        </div>
        <p className="workspace-intro">
          {firstName ? `Organise your projects in one calm space, ${firstName}. Capture new ideas, keep the list tidy, and jump back into any workspace instantly.` : "Organise your projects in one calm space. Capture new ideas, keep the list tidy, and jump back into any workspace instantly."}
        </p>
        <div className="workspace-metrics">
          <div className="workspace-metric">
            <span className="workspace-metric__label">Total projects</span>
            <span className="workspace-metric__value">{totalProjects}</span>
            <span className="workspace-metric__hint">Active across the workspace</span>
          </div>
        </div>
      </header>

  {bannerMessage && <div className="workspace-banner">{bannerMessage}</div>}

      <div className="workspace-body">
        <section className="block-card block-card--stretch">
          <div className="block-heading">
            <div>
              <h2 className="block-title">Projects</h2>
              <p className="block-subtitle">Every workspace you have access to appears here.</p>
            </div>
          </div>

          {isBooting ? (
            <div className="project-empty">Loading projects…</div>
          ) : projects.length === 0 ? (
            <div className="project-empty">
              <h3>Start your first project</h3>
              <p>Give it a title on the right and it will appear in this list.</p>
            </div>
          ) : (
            <div className="project-table">
              {projects.map((project) => (
                <article key={project.id} className="project-row">
                  <div className="project-row__main">
                    <Link to={`/projects/${project.id}`} className="project-row__title">
                      {project.title}
                    </Link>
                    {project.description && <p className="project-row__description">{project.description}</p>}
                    <span className="project-row__meta">Created {new Date(project.createdAt).toLocaleDateString()}</span>
                  </div>
                  <div className="project-row__actions">
                    <Link to={`/projects/${project.id}`} className="btn btn-ghost">Open</Link>
                    <button onClick={() => deleteProject(project.id)} className="btn btn-primary btn-primary--danger">Delete</button>
                  </div>
                </article>
              ))}
            </div>
          )}
        </section>

        <aside className="block-stack">
          <section className="block-card">
            <div className="block-heading">
              <div>
                <h2 className="block-title">Quick capture</h2>
                <p className="block-subtitle">Create a blank canvas and fill in the details later.</p>
              </div>
            </div>
            <form className="quick-form" onSubmit={(e) => { e.preventDefault(); createProject(); }}>
              <label className="field-label">
                <span className="field-title">Project title</span>
                <input
                  id="project-title-input"
                  ref={titleFieldRef}
                  className="input"
                  placeholder="Marketing launch plan"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </label>
              <label className="field-label">
                <span className="field-title">Description</span>
                <textarea
                  className="input input--textarea"
                  placeholder="Short summary for the team"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={3}
                />
              </label>
              <div className="form-actions">
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? "Creating..." : "Add project"}
                </button>
              </div>
              {creationError && <div className="form-alert">{creationError}</div>}
            </form>
          </section>

          <section className="block-card block-card--muted">
            <h3 className="hint-title">Workflow tip</h3>
            <p className="hint-copy">
              Structure each project with tasks and dependencies, then run the scheduler to build an ordered plan. Keep descriptions concise so teammates understand at a glance.
            </p>
          </section>
        </aside>
      </div>
    </div>
  );
}
