import axios from "axios";

/**
 * Single Axios instance — all requests go through API Gateway (Ocelot).
 *
 * Gateway (port 5050) routes:
 *   /api/auth/*   → UserService
 *   /api/admin/*  → UserService
 *   /api/url/*    → URLService
 *   /{code}       → URLService (redirect)
 */
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5050",
  timeout: 15_000,
  headers: { "Content-Type": "application/json" },
});

// Attach JWT token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("ql_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auto-logout on 401
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem("ql_token");
      localStorage.removeItem("ql_user");
      if (!window.location.pathname.startsWith("/login")) {
        window.location.href = "/login";
      }
    }
    return Promise.reject(err);
  }
);

export default api;