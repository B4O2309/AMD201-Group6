import React from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

const S = {
  bar: {
    position: "sticky",
    top: 0,
    zIndex: 100,
    height: 60,
    background: "var(--topbar)",
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    padding: "0 24px",
    boxShadow: "0 4px 20px rgba(0,0,0,0.18)",
  },
  brand: {
    display: "flex",
    alignItems: "center",
    gap: 10,
    color: "#fff",
    fontWeight: 900,
    fontSize: 20,
    letterSpacing: "-0.3px",
    textDecoration: "none",
  },
  logo: {
    width: 32,
    height: 32,
    borderRadius: 10,
    background: "var(--accent)",
    display: "grid",
    placeItems: "center",
    fontSize: 16,
    fontWeight: 900,
    color: "#fff",
  },
  right: {
    display: "flex",
    alignItems: "center",
    gap: 8,
  },
  navLink: {
    padding: "8px 14px",
    borderRadius: 8,
    color: "rgba(255,255,255,0.8)",
    fontWeight: 600,
    fontSize: 14,
    transition: "all 180ms ease",
    cursor: "pointer",
    border: "none",
    background: "transparent",
    textDecoration: "none",
  },
  navLinkActive: {
    color: "#fff",
    background: "rgba(255,255,255,0.1)",
  },
  userInfo: {
    display: "flex",
    alignItems: "center",
    gap: 6,
    marginRight: 6,
  },
  username: {
    color: "rgba(255,255,255,0.85)",
    fontSize: 13,
    fontWeight: 700,
  },
  roleBadge: {
    padding: "2px 8px",
    borderRadius: 6,
    fontSize: 11,
    fontWeight: 800,
    textTransform: "uppercase",
    letterSpacing: "0.3px",
  },
  logoutBtn: {
    padding: "8px 14px",
    borderRadius: 8,
    border: "1px solid rgba(255,255,255,0.15)",
    background: "transparent",
    color: "rgba(255,255,255,0.85)",
    fontWeight: 700,
    fontSize: 14,
    cursor: "pointer",
    transition: "all 180ms ease",
  },
};

export default function Navbar() {
  const { isAuthed, user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  const isActive = (path) => location.pathname === path;
  const isAdmin = (user?.role || "").toLowerCase() === "admin";

  const roleBadgeStyle = {
    ...S.roleBadge,
    background: isAdmin ? "var(--accent)" : "rgba(255,255,255,0.12)",
    color: isAdmin ? "#fff" : "rgba(255,255,255,0.7)",
  };

  return (
    <nav style={S.bar}>
      <Link to="/" style={S.brand}>
        <div style={S.logo}>Q</div>
        QuickLink
      </Link>

      <div style={S.right}>
        {isAuthed ? (
          <>
            <Link
              to="/"
              style={{ ...S.navLink, ...(isActive("/") ? S.navLinkActive : {}) }}
            >
              Home
            </Link>
            <Link
              to="/dashboard"
              style={{ ...S.navLink, ...(isActive("/dashboard") ? S.navLinkActive : {}) }}
            >
              List
            </Link>

            {/* Admin link — only visible to Admin role */}
            {isAdmin && (
              <Link
                to="/admin"
                style={{
                  ...S.navLink,
                  ...(isActive("/admin") ? S.navLinkActive : {}),
                  color: isActive("/admin") ? "var(--accent)" : "rgba(250,129,18,0.85)",
                  fontWeight: 800,
                }}
              >
                Admin
              </Link>
            )}

            <div style={S.userInfo}>
              <span style={S.username}>
                {user?.username || user?.email || "User"}
              </span>
              <span style={roleBadgeStyle}>
                {user?.role || "User"}
              </span>
            </div>

            <button
              type="button"
              onClick={handleLogout}
              style={S.logoutBtn}
              onMouseEnter={(e) => (e.target.style.background = "rgba(255,255,255,0.08)")}
              onMouseLeave={(e) => (e.target.style.background = "transparent")}
            >
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" style={S.navLink}>
              Login
            </Link>
            <Link
              to="/register"
              style={{
                ...S.navLink,
                background: "var(--accent)",
                color: "#fff",
                fontWeight: 800,
                borderRadius: 8,
              }}
            >
              Sign Up
            </Link>
          </>
        )}
      </div>
    </nav>
  );
}