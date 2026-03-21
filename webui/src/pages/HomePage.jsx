import React, { useState } from "react";
import Navbar from "../components/Navbar.jsx";
import ShortenForm from "../components/ShortenForm.jsx";

export default function HomePage() {
  const [recentLinks, setRecentLinks] = useState([]);

  function handleCreated(link) {
    setRecentLinks((prev) => [link, ...prev].slice(0, 5));
  }

  return (
    <div style={{ minHeight: "100vh", background: "var(--bg)" }}>
      <Navbar />

      <main style={{ maxWidth: 960, margin: "0 auto", padding: "48px 20px 80px" }}>
        {/* Hero */}
        <div style={{ textAlign: "center", marginBottom: 40 }}>
          <div
            style={{
              width: 72,
              height: 72,
              borderRadius: "50%",
              background: "var(--accent-light)",
              display: "grid",
              placeItems: "center",
              margin: "0 auto 18px",
              fontSize: 32,
            }}
          >
            🔗
          </div>

          <h1
            style={{
              fontFamily: "var(--font-display)",
              fontSize: "clamp(36px, 5vw, 56px)",
              fontWeight: 900,
              lineHeight: 1.08,
              margin: 0,
              letterSpacing: "-0.5px",
            }}
          >
            Shorten links,{" "}
            <span style={{ color: "var(--accent)" }}>share faster</span>
          </h1>

          <p
            style={{
              marginTop: 14,
              fontSize: 18,
              color: "var(--text-secondary)",
              maxWidth: 540,
              marginLeft: "auto",
              marginRight: "auto",
              lineHeight: 1.5,
            }}
          >
            Turn long URLs into short, trackable links in one click.
          </p>
        </div>

        {/* Shorten Form */}
        <div style={{ maxWidth: 900, margin: "0 auto", width: "100%" }}>
          <ShortenForm onCreated={handleCreated} />
        </div>

        {/* Recent links (session only) */}
        {recentLinks.length > 0 && (
          <div className="card" style={{ maxWidth: 900, margin: "24px auto 0" }}>
            <h3 style={{ margin: "0 0 12px", fontSize: 16, fontWeight: 800 }}>
              Just created
            </h3>
            <div style={{ display: "grid", gap: 8 }}>
              {recentLinks.map((link) => (
                <div
                  key={link.id}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    padding: "12px 14px",
                    borderRadius: "var(--radius-sm)",
                    background: "var(--accent-light)",
                    border: "1px solid var(--border)",
                    gap: 12,
                    flexWrap: "wrap",
                  }}
                >
                  <div style={{ minWidth: 0, flex: 1 }}>
                    <div style={{ fontWeight: 800, color: "var(--accent)", fontSize: 15 }}>
                      <a href={link.shortUrl} target="_blank" rel="noreferrer" style={{ color: "var(--accent)" }}>
                        {link.shortUrl}
                      </a>
                    </div>
                    <div
                      style={{
                        fontSize: 13,
                        color: "var(--text-secondary)",
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        whiteSpace: "nowrap",
                      }}
                    >
                      {link.originalUrl}
                    </div>
                  </div>
                  <button
                    type="button"
                    className="btn"
                    style={{ padding: "6px 12px", fontSize: 13 }}
                    onClick={async () => {
                      try {
                        await navigator.clipboard.writeText(link.shortUrl);
                      } catch { /* noop */ }
                    }}
                  >
                    Copy
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Features */}
        <div
          style={{
            marginTop: 56,
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
            gap: 20,
            maxWidth: 900,
            marginLeft: "auto",
            marginRight: "auto",
          }}
        >
          {[
            { icon: "⚡", title: "Instant", desc: "Shorten any URL in under a second" },
            { icon: "📊", title: "Analytics", desc: "Track click counts on every link" },
            { icon: "🔒", title: "Secure", desc: "JWT-protected, your links stay private" },
          ].map((f) => (
            <div
              key={f.title}
              className="card"
              style={{ textAlign: "center", padding: 28 }}
            >
              <div style={{ fontSize: 32, marginBottom: 10 }}>{f.icon}</div>
              <div style={{ fontSize: 18, fontWeight: 900, marginBottom: 6 }}>
                {f.title}
              </div>
              <div style={{ color: "var(--text-secondary)", lineHeight: 1.4 }}>
                {f.desc}
              </div>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}