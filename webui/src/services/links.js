import { urlApi } from "./api.js";

/**
 * URL Service endpoints (URLController.cs)
 *
 *   POST   /api/url/shorten            [Authorize]         → { shortUrl }
 *   GET    /api/url/my-urls            [Authorize]         → [{ id, longUrl, shortCode, clickCount, createdAt, shortUrl }]
 *   DELETE /api/url/my-urls/{urlId}    [Authorize]         → { message }
 *   GET    /{code}                     public              → 302 redirect
 */

/**
 * POST /api/url/shorten
 * Body: { longUrl: "https://..." }
 * Response: { shortUrl: "http://host/abc1234" }
 */
export async function shortenUrl(longUrl) {
  const res = await urlApi.post("/api/url/shorten", { longUrl });
  return res.data;
}

/**
 * GET /api/url/my-urls
 * Returns URLs created by the currently logged-in user (UserId from JWT).
 */
export async function getMyLinks() {
  const res = await urlApi.get("/api/url/my-urls");
  return Array.isArray(res.data) ? res.data : [];
}

/**
 * DELETE /api/url/my-urls/{urlId}
 * User can only delete their own URLs.
 */
export async function deleteLink(urlId) {
  const res = await urlApi.delete(`/api/url/my-urls/${urlId}`);
  return res.data;
}
