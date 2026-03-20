import React, {
  createContext, useContext, useMemo, useState, useCallback, useEffect,
} from "react";
import { userApi } from "../services/api.js";

const AuthContext = createContext(null);

function loadUser() {
  try { return JSON.parse(localStorage.getItem("ql_user")); } catch { return null; }
}
function loadToken() { return localStorage.getItem("ql_token") || null; }
function persist(token, user) {
  localStorage.setItem("ql_token", token);
  localStorage.setItem("ql_user", JSON.stringify(user));
}
function clearStorage() {
  localStorage.removeItem("ql_token");
  localStorage.removeItem("ql_user");
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadUser);
  const [token, setToken] = useState(loadToken);
  const [loading, setLoading] = useState(!!loadToken());
  const isAuthed = Boolean(token);

  /**
   * POST /api/auth/login
   * Body: { email, password }
   * Response: { token, user: { id, username, email, role, createdAt } }
   */
  const login = useCallback(async ({ email, password }) => {
    const res = await userApi.post("/api/auth/login", { email, password });
    const { token: jwt, user: usr } = res.data;
    if (!jwt) throw new Error("Server did not return a token.");
    persist(jwt, usr);
    setToken(jwt);
    setUser(usr);
    return usr;
  }, []);

  /**
   * POST /api/auth/register
   * Body: { username, email, password, role: "User" }
   * Note: password min 8 chars (backend validation)
   */
  const register = useCallback(async ({ username, email, password }) => {
    await userApi.post("/api/auth/register", {
      username, email, password, role: "User",
    });
  }, []);

  /**
   * GET /api/auth/me
   * Response: { id, email, username, role }
   */
  const fetchMe = useCallback(async () => {
    try {
      const res = await userApi.get("/api/auth/me");
      localStorage.setItem("ql_user", JSON.stringify(res.data));
      setUser(res.data);
    } catch {
      if (token && token.startsWith("dev_token_")) return;
      clearStorage(); setToken(null); setUser(null);
    }
  }, [token]);

  /** Dev login bypass — remove when both services are running */
  const devLogin = useCallback((role = "Admin") => {
    const fakeToken = "dev_token_" + Date.now();
    const fakeUser = {
      id: 0,
      username: role === "Admin" ? "admin_dev" : "user_dev",
      email: role === "Admin" ? "admin@dev.local" : "user@dev.local",
      role,
    };
    persist(fakeToken, fakeUser);
    setToken(fakeToken);
    setUser(fakeUser);
    return fakeUser;
  }, []);

  const logout = useCallback(() => {
    clearStorage(); setToken(null); setUser(null);
  }, []);

  useEffect(() => {
    if (!token) { setLoading(false); return; }
    if (token.startsWith("dev_token_")) { setLoading(false); return; }
    setLoading(true);
    fetchMe().finally(() => setLoading(false));
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const value = useMemo(
    () => ({ user, token, isAuthed, loading, login, devLogin, register, logout, fetchMe }),
    [user, token, isAuthed, loading, login, devLogin, register, logout, fetchMe]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside <AuthProvider>");
  return ctx;
}
