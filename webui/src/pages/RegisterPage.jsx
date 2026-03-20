import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export default function RegisterPage() {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    if (username.trim().length < 3) { setError("Username must be at least 3 characters."); return; }
    if (password.length < 8) { setError("Password must be at least 8 characters."); return; }
    if (password !== confirm) { setError("Passwords do not match."); return; }
    setLoading(true);
    try {
      await register({ username: username.trim(), email, password });
      navigate("/login", { replace: true });
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data || err.message || "Registration failed.";
      setError(typeof msg === "string" ? msg : JSON.stringify(msg));
    } finally { setLoading(false); }
  }

  return (
    <div style={S.page}>
      <div style={S.card}>
        <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 24 }}>
          <div style={S.logo}>Q</div>
          <div>
            <h1 style={{ margin: 0, fontSize: 24, fontWeight: 900, fontFamily: "var(--font-display)" }}>Create account</h1>
            <p style={{ margin: "4px 0 0", fontSize: 14, color: "var(--text-secondary)" }}>Sign up to start shortening URLs</p>
          </div>
        </div>
        <form onSubmit={handleSubmit} style={{ display: "grid", gap: 14 }}>
          <label style={S.label}>Username<input className="input" type="text" placeholder="e.g. john_doe" value={username} onChange={e => setUsername(e.target.value)} required disabled={loading} autoComplete="username" /></label>
          <label style={S.label}>Email<input className="input" type="email" placeholder="you@example.com" value={email} onChange={e => setEmail(e.target.value)} required disabled={loading} autoComplete="email" /></label>
          <label style={S.label}>Password (min 8 chars)<input className="input" type="password" placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} required disabled={loading} autoComplete="new-password" /></label>
          <label style={S.label}>Confirm password<input className="input" type="password" placeholder="••••••••" value={confirm} onChange={e => setConfirm(e.target.value)} required disabled={loading} autoComplete="new-password" /></label>
          {error && <div className="alert alert-error">{error}</div>}
          <button type="submit" className="btn btn-primary" style={{ width: "100%", padding: "14px 18px", fontSize: 15 }} disabled={loading}>{loading ? "Creating account…" : "Create account"}</button>
          <div style={{ display: "flex", justifyContent: "center", gap: 8, fontSize: 14, color: "var(--text-secondary)" }}>
            <span>Already have an account?</span>
            <Link to="/login" style={{ color: "var(--accent)", fontWeight: 800 }}>Sign in</Link>
          </div>
        </form>
      </div>
    </div>
  );
}

const S = {
  page: { minHeight: "100vh", display: "grid", placeItems: "center", padding: 24, background: "radial-gradient(ellipse 800px 400px at 25% 15%, rgba(250,129,18,0.14), transparent 60%), radial-gradient(ellipse 700px 500px at 85% 40%, rgba(245,231,198,0.8), transparent 50%), var(--bg)" },
  card: { width: "100%", maxWidth: 420, background: "var(--surface)", border: "1px solid var(--border)", borderRadius: "var(--radius-lg)", padding: 28, backdropFilter: "blur(12px)", boxShadow: "var(--shadow-lg)" },
  logo: { width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", fontWeight: 900, fontSize: 20, background: "var(--bg-deep)", border: "1px solid var(--border)", color: "var(--text)" },
  label: { display: "grid", gap: 6, fontSize: 13, fontWeight: 700, color: "var(--text)" },
};
