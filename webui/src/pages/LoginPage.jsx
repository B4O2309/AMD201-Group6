import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const { login, devLogin } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setError(""); setLoading(true);
    try {
      await login({ email, password });
      navigate("/", { replace: true });
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data || err.message || "Login failed.";
      setError(typeof msg === "string" ? msg : JSON.stringify(msg));
    } finally { setLoading(false); }
  }

  function handleDev(role) { devLogin(role); navigate("/", { replace: true }); }

  return (
    <div style={S.page}>
      <div style={S.card}>
        <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 24 }}>
          <div style={S.logo}>Q</div>
          <div>
            <h1 style={{ margin: 0, fontSize: 24, fontWeight: 900, fontFamily: "var(--font-display)" }}>Welcome back</h1>
            <p style={{ margin: "4px 0 0", fontSize: 14, color: "var(--text-secondary)" }}>Sign in to your account</p>
          </div>
        </div>
        <form onSubmit={handleSubmit} style={{ display: "grid", gap: 14 }}>
          <label style={S.label}>Email<input className="input" type="email" placeholder="you@example.com" value={email} onChange={e => setEmail(e.target.value)} required disabled={loading} autoComplete="email" /></label>
          <label style={S.label}>Password<input className="input" type="password" placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} required disabled={loading} autoComplete="current-password" /></label>
          {error && <div className="alert alert-error">{error}</div>}
          <button type="submit" className="btn btn-primary" style={{ width: "100%", padding: "14px 18px", fontSize: 15 }} disabled={loading}>{loading ? "Signing in…" : "Sign in"}</button>
          <div style={{ display: "flex", justifyContent: "center", gap: 8, fontSize: 14, color: "var(--text-secondary)" }}>
            <span>Don't have an account?</span>
            <Link to="/register" style={{ color: "var(--accent)", fontWeight: 800 }}>Sign up</Link>
          </div>
        </form>
        <div style={{ marginTop: 20, paddingTop: 16, borderTop: "1px dashed var(--border-strong)" }}>
          <p style={{ fontSize: 12, color: "var(--text-secondary)", textAlign: "center", marginBottom: 10, fontWeight: 600 }}>Dev Mode — bypass auth</p>
          <div style={{ display: "flex", gap: 8 }}>
            <button type="button" className="btn" style={{ flex: 1, fontSize: 13 }} onClick={() => handleDev("Admin")}>Dev Login (Admin)</button>
            <button type="button" className="btn" style={{ flex: 1, fontSize: 13 }} onClick={() => handleDev("User")}>Dev Login (User)</button>
          </div>
        </div>
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
