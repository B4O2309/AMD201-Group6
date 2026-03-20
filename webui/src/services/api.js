import axios from "axios";

/**
 * Two separate Axios instances — one per microservice.
 * No API Gateway exists yet, so frontend calls each service directly.
 *
 * UserService (port 5001): auth, admin
 * URLService  (port 5000): shorten, my-urls, redirect
 */

function createClient(baseURL) {
  const client = axios.create({
    baseURL,
    timeout: 15_000,
    headers: { "Content-Type": "application/json" },
  });

  // Attach JWT token to every request
  client.interceptors.request.use((config) => {
    const token = localStorage.getItem("ql_token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  // Auto-logout on 401
  client.interceptors.response.use(
    (res) => res,
    (err) => {
      if (err.response?.status === 401) {
        const token = localStorage.getItem("ql_token");
        // Don't auto-logout for dev tokens
        if (token && !token.startsWith("dev_token_")) {
          localStorage.removeItem("ql_token");
          localStorage.removeItem("ql_user");
          if (!window.location.pathname.startsWith("/login")) {
            window.location.href = "/login";
          }
        }
      }
      return Promise.reject(err);
    }
  );

  return client;
}

// UserService — auth + admin endpoints
export const userApi = createClient(
  import.meta.env.VITE_USER_SERVICE_URL || "http://localhost:5001"
);

// URLService — shorten + my-urls endpoints
export const urlApi = createClient(
  import.meta.env.VITE_URL_SERVICE_URL || "http://localhost:5000"
);
