import React, { useState } from "react";
import { Outlet, Link, useNavigate, useLocation } from "react-router-dom";

export default function Layout() {
  const nav = useNavigate();
  const location = useLocation();
  const token = localStorage.getItem("token");
  const profileName = localStorage.getItem("profileName");
  const firstName = profileName ? profileName.trim().split(" ")[0] : null;
  const [open, setOpen] = useState(false);

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("profileName");
    localStorage.removeItem("profileEmail");
    nav("/login");
  };

  const handleNavigate = (path: string) => {
    setOpen(false);
    nav(path);
  };

  return (
    <div className="page-root min-h-screen flex flex-col">
      <header className="layout-header">
        <div className="app-shell layout-header__inner">
          <div className="brand-group">
            <span className="brand-badge">PM</span>
            <div className="brand-copy">
              <Link to={token ? "/dashboard" : "/login"} className="brand-name">Project Manager</Link>
              <span className="brand-subtitle">Plan with clarity. Deliver with confidence.</span>
            </div>
          </div>

          <nav className="desktop-nav">
            {token ? (
              <>
                {firstName && <span className="nav-user">Hi, {firstName}</span>}
                <button
                  onClick={() => handleNavigate("/dashboard")}
                  className={`nav-link ${location.pathname.startsWith("/dashboard") ? "nav-link--active" : ""}`}
                >
                  Dashboard
                </button>
                <button onClick={logout} className="btn btn-primary text-sm">Logout</button>
              </>
            ) : (
              <>
                <Link to="/login" className={`nav-link ${location.pathname === "/login" ? "nav-link--active" : ""}`}>Login</Link>
                <Link to="/register" className="btn btn-primary text-sm">Create account</Link>
              </>
            )}
          </nav>

          <button
            onClick={() => setOpen(!open)}
            aria-label="Toggle navigation"
            className="mobile-toggle"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {open ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>
        </div>

        {open && (
          <div className="mobile-menu">
            {token ? (
              <>
                {firstName && <span className="nav-user nav-user--mobile">Hi, {firstName}</span>}
                <button onClick={() => handleNavigate("/dashboard")} className="nav-link nav-link--mobile">Dashboard</button>
                <button onClick={logout} className="btn btn-primary w-full justify-center">Logout</button>
              </>
            ) : (
              <>
                <Link to="/login" onClick={() => setOpen(false)} className="nav-link nav-link--mobile">Login</Link>
                <Link to="/register" onClick={() => setOpen(false)} className="btn btn-primary w-full justify-center">Create account</Link>
              </>
            )}
          </div>
        )}
      </header>

      <main className="app-shell flex-1 w-full py-12">
        <Outlet />
      </main>

      <footer className="app-shell footer">
        <span>© {new Date().getFullYear()} Project Manager</span>
      </footer>
    </div>
  );
}
