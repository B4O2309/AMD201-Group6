import React, { useState } from "react";
import { shortenUrl } from "../services/links.js";

export default function ShortenForm({ onCreated }) {
  const [url, setUrl] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(""); setResult(null);
    const trimmed = url.trim();
    if (!trimmed) return;
    try { new URL(trimmed); } catch { setError("Please enter a valid URL (e.g. https://example.com)"); return; }

    setLoading(true);
    try {
      const data = await shortenUrl(trimmed);
      setResult(data);
      setUrl("");
      onCreated?.(data);
      setTimeout(() => setResult(null), 8000);
    } catch (err) {
      const status = err.response?.status;
      const data = err.response?.data;
      if (status === 400) setError(typeof data === "string" ? data : "Invalid URL format.");
      else if (status === 401) setError("Login required. Dev tokens won't work — please login with a real account.");
      else setError(typeof (data?.message || data) === "string" ? (data?.message || data) : "Could not shorten URL.");
    } finally { setLoading(false); }
  }

  const shortUrl = result?.shortUrl || result?.ShortUrl || "";

  return (
    <div className="card" style={{ width: "100%" }}>
      <h3 style={{ margin: "0 0 14px", fontSize: 18, fontWeight: 800 }}>Shorten a URL</h3>
      <form onSubmit={handleSubmit} style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
        <input className="input" style={{ flex: "1 1 300px", minWidth: 0 }} placeholder="Paste a long URL here (https://...)" value={url} onChange={e => setUrl(e.target.value)} disabled={loading} />
        <button type="submit" className="btn btn-primary" disabled={loading} style={{ minWidth: 120 }}>{loading ? "Shortening…" : "Shorten"}</button>
      </form>
      {error && <div className="alert alert-error" style={{ marginTop: 12 }}>{error}</div>}
      {shortUrl && (
        <div className="alert alert-success" style={{ marginTop: 12, display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: 8 }}>
          <span><strong>Shortened:</strong>{" "}<a href={shortUrl} target="_blank" rel="noreferrer" style={{ color: "inherit", textDecoration: "underline" }}>{shortUrl}</a></span>
          <button type="button" className="btn" style={{ padding: "4px 10px", fontSize: 12 }} onClick={async () => { try { await navigator.clipboard.writeText(shortUrl); } catch {} }}>Copy</button>
        </div>
      )}
    </div>
  );
}