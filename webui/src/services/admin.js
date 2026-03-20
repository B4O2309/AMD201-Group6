import { userApi } from "./api.js";

/**
 * Admin endpoints (AdminController.cs on UserService)
 * All require [Authorize(Roles = "Admin")]
 *
 *   GET    /api/admin/users                    → [{ id, username, email, role, createdAt }]
 *   DELETE /api/admin/users/{userId}           → { message }
 *   GET    /api/admin/users/{userId}/urls      → proxied to URLService
 *   DELETE /api/admin/urls/{urlId}             → proxied to URLService
 */

/** GET /api/admin/users — all registered users */
export async function adminGetUsers() {
  const res = await userApi.get("/api/admin/users");
  return Array.isArray(res.data) ? res.data : [];
}

/** DELETE /api/admin/users/{userId} — delete a user */
export async function adminDeleteUser(userId) {
  const res = await userApi.delete(`/api/admin/users/${userId}`);
  return res.data;
}

/** GET /api/admin/users/{userId}/urls — view a user's URLs (proxied to URLService) */
export async function adminGetUserLinks(userId) {
  const res = await userApi.get(`/api/admin/users/${userId}/urls`);
  return Array.isArray(res.data) ? res.data : [];
}

/** DELETE /api/admin/urls/{urlId} — delete any URL (proxied to URLService) */
export async function adminDeleteLink(urlId) {
  const res = await userApi.delete(`/api/admin/urls/${urlId}`);
  return res.data;
}
