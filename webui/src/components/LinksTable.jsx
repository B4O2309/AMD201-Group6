import React, { useState } from "react";

function clamp(text, max = 60) {
  if (!text) return "—";
  return text.length > max ? text.slice(0, max - 1) + "…" : text;
}

export default function LinksTable({
  links = [],
  loading = false,
  onDelete,
  showOwner = false,
}) {
  const [copiedId, setCopiedId] = useState(null);

  async function handleCopy(shortUrl, id) {
    try {
      await navigator.clipboard.writeText(shortUrl);
      setCopiedId(id);
      setTimeout(() => setCopiedId(null), 1500);
    } catch { /* noop */ }
  }

  if (loading) {
    return (
      <div style={{ padding: 24, textAlign: "center", color: "var(--text-secondary)" }}>
        Loading links…
      </div>
    );
  }

  if (links.length === 0) {
    return (
      <div style={{ padding: 24, textAlign: "center", color: "var(--text-secondary)" }}>
        No links found.
      </div>
    );
  }

  return (
    <div style={{ overflowX: "auto" }}>
      <table className="tbl">
        <thead>
          <tr>
            {showOwner && <th>Owner</th>}
            <th>Original URL</th>
            <th>Short Link</th>
            <th style={{ width: 70 }}>Clicks</th>
            <th style={{ width: 100 }}>Created</th>
            <th style={{ width: 160 }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {links.map((link) => {
            const id = link.id || link.Id;
            const shortUrl = link.shortUrl || link.ShortUrl || link.shortenedUrl || "";
            const originalUrl = link.originalUrl || link.OriginalUrl || link.url || link.longUrl || link.LongUrl || "";
            const clicks = link.clicks ?? link.Clicks ?? link.clickCount ?? link.ClickCount ?? 0;
            const created = link.createdAt || link.CreatedAt || "";
            const owner = link.ownerEmail || link.userEmail || link.email || "";

            return (
              <tr key={id}>
                {showOwner && (
                  <td style={{ fontSize: 13, fontWeight: 700 }}>
                    {owner || "—"}
                  </td>
                )}
                <td title={originalUrl}>
                  <a href={originalUrl} target="_blank" rel="noreferrer">
                    {clamp(originalUrl, 50)}
                  </a>
                </td>
                <td title={shortUrl}>
                  <a
                    href={shortUrl}
                    target="_blank"
                    rel="noreferrer"
                    style={{ color: "var(--accent)", fontWeight: 800 }}
                  >
                    {shortUrl || "—"}
                  </a>
                </td>
                <td>{clicks}</td>
                <td style={{ fontSize: 13, color: "var(--text-secondary)" }}>
                  {created ? new Date(created).toLocaleDateString() : "—"}
                </td>
                <td>
                  <div style={{ display: "flex", gap: 6, flexWrap: "wrap" }}>
                    <button
                      type="button"
                      className="btn"
                      style={{ padding: "6px 10px", fontSize: 13 }}
                      onClick={() => handleCopy(shortUrl, id)}
                      disabled={!shortUrl}
                    >
                      {copiedId === id ? "Copied!" : "Copy"}
                    </button>

                    {onDelete && (
                      <button
                        type="button"
                        className="btn btn-danger"
                        style={{ padding: "6px 10px", fontSize: 13 }}
                        onClick={() => {
                          if (confirm("Delete this link?")) onDelete(id);
                        }}
                      >
                        Delete
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
