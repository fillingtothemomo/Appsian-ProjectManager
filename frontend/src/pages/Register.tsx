import React, { useState } from "react";
import api from "../api";
import { useNavigate, Link } from "react-router-dom";
import FormInput from "../components/FormInput";

type AuthResponse = {
  token: string;
  name: string;
  email: string;
};

export default function Register() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [formError, setFormError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setFormError(null);
    const trimmedName = name.trim();
    const trimmedEmail = email.trim();
    if (!trimmedName || !trimmedEmail || !password) {
      setFormError("Fill all fields");
      return;
    }
    if (password.length < 6) {
      setFormError("Password must be >=6 chars");
      return;
    }
    setLoading(true);
    try {
      const res = await api.post<AuthResponse>("/auth/register", {
        name: trimmedName,
        email: trimmedEmail,
        password,
      });
      localStorage.setItem("token", res.data.token);
      localStorage.setItem("profileName", res.data.name);
      localStorage.setItem("profileEmail", res.data.email);
      navigate("/dashboard");
    } catch (err: any) {
      setFormError(err?.response?.data?.error || "Registration failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <section className="auth-card">
        <span className="auth-badge">Project Manager</span>
        <h1 className="auth-title">Create your workspace</h1>
        <p className="auth-subtitle">Set up your account to start planning.</p>

        <form onSubmit={handleSubmit} className="auth-form">
          <FormInput label="Your name" value={name} onChange={setName} placeholder="Lady Ada Lovelace" />
          <FormInput label="Email" value={email} onChange={setEmail} type="email" placeholder="you@example.com" />
          <FormInput label="Password" value={password} onChange={setPassword} type="password" />
          {formError && <div className="form-alert form-alert--inline">{formError}</div>}
          <button className="btn btn-primary auth-submit" disabled={loading}>{loading ? "Creating..." : "Create account"}</button>
        </form>

        <p className="auth-switch">
          Already onboard? <Link to="/login" className="auth-switch__link">Sign in</Link>
        </p>
      </section>

      <aside className="auth-aside">
        <div className="auth-aside__content">
          <h2>Appsian Tech Project Management Assignment</h2>
          <p>Made by Angel Sharma , 22323005</p>
        </div>
      </aside>
    </div>
  );
}
