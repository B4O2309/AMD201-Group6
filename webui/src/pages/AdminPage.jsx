import React, { useEffect, useState, useCallback } from "react";
import Navbar from "../components/Navbar.jsx";
import LinksTable from "../components/LinksTable.jsx";
import {
  adminGetUsers,
  adminDeleteUser,
  adminGetUserLinks,
  adminDeleteLink,
} from "../services/admin.js";

export default function AdminPage() {
  const [users, setUsers] = useState([]);
  const [usersLoading, setUsersLoading] = useState(true);
  const [usersError, setUsersError] = useState("");

  // Selected user's links
  const [selectedUser, setSelectedUser] = useState(null);
  const [userLinks, setUserLinks] = useState([]);
  const [linksLoading, setLinksLoading] = useState(false);
  const [linksError, setLinksError] = useState("");

  const fetchUsers = useCallback(async () => {
    setUsersLoading(true); setUsersError("");
    try {
      setUsers(await adminGetUsers());
    } catch (err) {
      const s = err.response?.status;
      if (s === 401 || s === 403) setUsersError("Admin login required. Dev tokens won't work — login with a real Admin account.");
      else setUsersError(err.response?.data?.message || err.message || "Failed to load users.");
    } finally { setUsersLoading(false); }
  }, []);

  useEffect(() => { fetchUsers(); }, [fetchUsers]);

  async function handleDeleteUser(userId) {
    if (!confirm("Delete this user? This cannot be undone.")) return;
    try {
      await adminDeleteUser(userId);
    } catch (err) {
      alert("Delete failed: " + (err.response?.data?.message || err.message));
    }
    // If we were viewing this user's links, clear selection
    if (selectedUser?.id === userId) { setSelectedUser(null); setUserLinks([]); }
    fetchUsers();
  }

  async function handleViewUserLinks(user) {
    setSelectedUser(user);
    setLinksLoading(true); setLinksError("");
    try {
      setUserLinks(await adminGetUserLinks(user.id || user.Id));
    } catch (err) {
      const s = err.response?.status;
      if (s === 503) setLinksError("URL Service is currently unavailable.");
      else setLinksError(err.response?.data?.message || err.message || "Failed to load user's links.");
      setUserLinks([]);
    } finally { setLinksLoading(false); }
  }

  async function handleDeleteLink(urlId) {
    try {
      await adminDeleteLink(urlId);
    } catch (err) {
      alert("Delete failed: " + (err.response?.data?.message || err.message));
    }
    // Refresh links for the selected user
    if (selectedUser) handleViewUserLinks(selectedUser);
  }

  return (
    <div style={{ minHeight: "100vh", background: "var(--bg)" }}>
      <Navbar />
      <main style={{ maxWidth: 1060, margin: "0 auto", padding: "32px 20px 80px" }}>
        <div style={{ marginBottom: 28 }}>
          <h1 style={{ margin: 0, fontSize: 32, fontWeight: 900, fontFamily: "var(--font-display)" }}>Admin Panel</h1>
          <p style={{ margin: "6px 0 0", color: "var(--text-secondary)", fontWeight: 600 }}>Manage all users and their links</p>
        </div>

        {/* ── Users Table ── */}
        <div className="card" style={{ marginBottom: 24 }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 14 }}>
            <h2 style={{ margin: 0, fontSize: 18, fontWeight: 900 }}>All Users</h2>
            <button type="button" className="btn" onClick={fetchUsers}>↻ Refresh</button>
          </div>

          {usersError && <div className="alert alert-error" style={{ marginBottom: 14 }}>{usersError}</div>}

          {usersLoading ? (
            <div style={{ padding: 24, textAlign: "center", color: "var(--text-secondary)" }}>Loading users…</div>
          ) : users.length === 0 && !usersError ? (
            <div style={{ padding: 24, textAlign: "center", color: "var(--text-secondary)" }}>No users found.</div>
          ) : (
            <div style={{ overflowX: "auto" }}>
              <table className="tbl">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Username</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Created</th>
                    <th style={{ width: 200 }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((u) => {
                    const id = u.id || u.Id;
                    const isSelected = selectedUser && (selectedUser.id || selectedUser.Id) === id;
                    return (
                      <tr key={id} style={isSelected ? { background: "var(--accent-light)" } : {}}>
                        <td style={{ fontSize: 13, color: "var(--text-secondary)" }}>{id}</td>
                        <td style={{ fontWeight: 700 }}>{u.username || u.Username || "—"}</td>
                        <td>{u.email || u.Email || "—"}</td>
                        <td>
                          <span style={{
                            display: "inline-block", padding: "3px 10px", borderRadius: 6,
                            fontSize: 12, fontWeight: 800, textTransform: "uppercase", letterSpacing: "0.3px",
                            background: (u.role || u.Role) === "Admin" ? "var(--accent)" : "var(--accent-light)",
                            color: (u.role || u.Role) === "Admin" ? "#fff" : "var(--accent)",
                          }}>{u.role || u.Role || "User"}</span>
                        </td>
                        <td style={{ fontSize: 13, color: "var(--text-secondary)" }}>
                          {(u.createdAt || u.CreatedAt) ? new Date(u.createdAt || u.CreatedAt).toLocaleDateString() : "—"}
                        </td>
                        <td>
                          <div style={{ display: "flex", gap: 6 }}>
                            <button type="button" className="btn" style={{ padding: "6px 10px", fontSize: 12 }}
                              onClick={() => handleViewUserLinks(u)}>
                              {isSelected ? "Viewing…" : "View Links"}
                            </button>
                            <button type="button" className="btn btn-danger" style={{ padding: "6px 10px", fontSize: 12 }}
                              onClick={() => handleDeleteUser(id)}>
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* ── Selected User's Links ── */}
        {selectedUser && (
          <div className="card">
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 14 }}>
              <h2 style={{ margin: 0, fontSize: 18, fontWeight: 900 }}>
                Links of{" "}
                <span style={{ color: "var(--accent)" }}>
                  {selectedUser.username || selectedUser.Username || selectedUser.email || selectedUser.Email}
                </span>
              </h2>
              <button type="button" className="btn" style={{ fontSize: 13 }}
                onClick={() => { setSelectedUser(null); setUserLinks([]); }}>
                Close
              </button>
            </div>

            {linksError && <div className="alert alert-error" style={{ marginBottom: 14 }}>{linksError}</div>}

            <LinksTable
              links={userLinks}
              loading={linksLoading}
              onDelete={handleDeleteLink}
            />
          </div>
        )}
      </main>
    </div>
  );
}
