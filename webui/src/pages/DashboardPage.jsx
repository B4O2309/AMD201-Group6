import React, { useEffect, useState, useCallback } from "react";
import Navbar from "../components/Navbar.jsx";
import LinksTable from "../components/LinksTable.jsx";
import { getMyLinks, deleteLink } from "../services/links.js";

export default function DashboardPage() {
  const [links, setLinks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchLinks = useCallback(async () => {
    setLoading(true); setError("");
    try {
      setLinks(await getMyLinks());
    } catch (err) {
      const s = err.response?.status;
      if (s === 401) setError("Login required. Please login with a real account.");
      else setError(err.response?.data?.message || err.message || "Failed to load links.");
      setLinks([]);
    } finally { setLoading(false); }
  }, []);

  useEffect(() => { fetchLinks(); }, [fetchLinks]);

  async function handleDelete(id) {
    try { await deleteLink(id); } catch (err) { alert("Delete failed: " + (err.response?.data?.message || err.message)); }
    fetchLinks();
  }

  return (
    <div style={{ minHeight: "100vh", background: "var(--bg)" }}>
      <Navbar />
      <main style={{ maxWidth: 1060, margin: "0 auto", padding: "32px 20px 80px" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", flexWrap: "wrap", gap: 12, marginBottom: 24 }}>
          <div>
            <h1 style={{ margin: 0, fontSize: 32, fontWeight: 900, fontFamily: "var(--font-display)" }}>List</h1>
            <p style={{ margin: "6px 0 0", color: "var(--text-secondary)", fontWeight: 600 }}>All your shortened links</p>
          </div>
          <button type="button" className="btn" onClick={fetchLinks}>↻ Refresh</button>
        </div>

        <div className="card">
          <h2 style={{ margin: "0 0 14px", fontSize: 18, fontWeight: 900 }}>Your Links</h2>
          {error && <div className="alert alert-error" style={{ marginBottom: 14 }}>{error}</div>}
          <LinksTable links={links} loading={loading} onDelete={handleDelete} />
        </div>
      </main>
    </div>
  );
}