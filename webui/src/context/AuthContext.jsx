import React, {
  createContext, useContext, useMemo, useState, useCallback, useEffect,
} from "react";
import api from "../services/api.js";

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
   * POST /api/auth/login  (Gateway → UserService)
   * Response: { token, user: { id, username, email, role, createdAt } }
   */
  const login = useCallback(async ({ email, password }) => {
    const res = await api.post("/api/auth/login", { email, password });
    const { token: jwt, user: usr } = res.data;
    if (!jwt) throw new Error("Server did not return a token.");
    persist(jwt, usr);
    setToken(jwt);
    setUser(usr);
    return usr;
  }, []);

  /**
   * POST /api/auth/register  (Gateway → UserService)
   * Body: { username, email, password, role: "User" }
   */
  const register = useCallback(async ({ username, email, password }) => {
    await api.post("/api/auth/register", {
      username, email, password, role: "User",
    });
  }, []);

  /**
   * GET /api/auth/me  (Gateway → UserService)
   * Response: { id, email, username, role }
   */
  const fetchMe = useCallback(async () => {
    try {
      const res = await api.get("/api/auth/me");
      localStorage.setItem("ql_user", JSON.stringify(res.data));
      setUser(res.data);
    } catch {
      clearStorage(); setToken(null); setUser(null);
    }
  }, []);

  const logout = useCallback(() => {
    clearStorage(); setToken(null); setUser(null);
  }, []);

  useEffect(() => {
    if (!token) { setLoading(false); return; }
    setLoading(true);
    fetchMe().finally(() => setLoading(false));
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const value = useMemo(
    () => ({ user, token, isAuthed, loading, login, register, logout, fetchMe }),
    [user, token, isAuthed, loading, login, register, logout, fetchMe]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside <AuthProvider>");
  return ctx;
}