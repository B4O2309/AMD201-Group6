import api from "./api.js";

/**
 * URL Service endpoints (via Gateway → URLService)
 *
 *   POST   /api/url/shorten            [Authorize]
 *   GET    /api/url/my-urls            [Authorize]
 *   DELETE /api/url/my-urls/{urlId}    [Authorize]
 */

/**
 * Backend returns shortUrl with internal Docker host (e.g. http://urlservice:8080/abc123)
 * We need to replace it with the Gateway URL so users can actually click the link.
 */
const GATEWAY = import.meta.env.VITE_API_BASE_URL || "http://localhost:5050";

function fixShortUrl(shortUrl) {
  if (!shortUrl) return "";
  try {
    const url = new URL(shortUrl);
    // Extract just the path (e.g. "/abc123") and prepend gateway
    return GATEWAY + url.pathname;
  } catch {
    // If not a valid URL, assume it's just a code
    return GATEWAY + "/" + shortUrl;
  }
}

function fixLink(link) {
  return {
    ...link,
    shortUrl: fixShortUrl(link.shortUrl || link.ShortUrl),
  };
}

export async function shortenUrl(longUrl) {
  const res = await api.post("/api/url/shorten", { longUrl });
  return fixLink(res.data);
}

export async function getMyLinks() {
  const res = await api.get("/api/url/my-urls");
  const data = Array.isArray(res.data) ? res.data : [];
  return data.map(fixLink);
}

export async function deleteLink(urlId) {
  const res = await api.delete(`/api/url/my-urls/${urlId}`);
  return res.data;
}