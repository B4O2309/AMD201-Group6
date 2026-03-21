import api from "./api.js";

/**
 * Admin endpoints (via Gateway → UserService → proxy to URLService)
 * All require [Authorize(Roles = "Admin")]
 */

const GATEWAY = import.meta.env.VITE_API_BASE_URL || "http://localhost:5050";

function fixShortUrl(shortUrl) {
  if (!shortUrl) return "";
  try {
    const url = new URL(shortUrl);
    return GATEWAY + url.pathname;
  } catch {
    return GATEWAY + "/" + shortUrl;
  }
}

function fixLink(link) {
  return { ...link, shortUrl: fixShortUrl(link.shortUrl || link.ShortUrl) };
}

export async function adminGetUsers() {
  const res = await api.get("/api/admin/users");
  return Array.isArray(res.data) ? res.data : [];
}

export async function adminDeleteUser(userId) {
  const res = await api.delete(`/api/admin/users/${userId}`);
  return res.data;
}

export async function adminGetUserLinks(userId) {
  const res = await api.get(`/api/admin/users/${userId}/urls`);
  const data = Array.isArray(res.data) ? res.data : [];
  return data.map(fixLink);
}

export async function adminDeleteLink(urlId) {
  const res = await api.delete(`/api/admin/urls/${urlId}`);
  return res.data;
}