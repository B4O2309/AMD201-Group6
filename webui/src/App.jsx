import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { useAuth } from "./context/AuthContext.jsx";

import HomePage from "./pages/HomePage.jsx";
import LoginPage from "./pages/LoginPage.jsx";
import RegisterPage from "./pages/RegisterPage.jsx";
import DashboardPage from "./pages/DashboardPage.jsx";
import AdminPage from "./pages/AdminPage.jsx";

function LoadingScreen() {
  return (
    <div style={{ minHeight: "100vh", display: "grid", placeItems: "center", background: "var(--bg)", color: "var(--text-secondary)", fontSize: 15, fontWeight: 600 }}>
      Verifying session…
    </div>
  );
}

function ProtectedRoute({ children }) {
  const { isAuthed, loading } = useAuth();
  if (loading) return <LoadingScreen />;
  if (!isAuthed) return <Navigate to="/login" replace />;
  return children;
}

function GuestRoute({ children }) {
  const { isAuthed, loading } = useAuth();
  if (loading) return null;
  if (isAuthed) return <Navigate to="/" replace />;
  return children;
}

function AdminRoute({ children }) {
  const { isAuthed, loading, user } = useAuth();
  if (loading) return <LoadingScreen />;
  if (!isAuthed) return <Navigate to="/login" replace />;
  if ((user?.role || "").toLowerCase() !== "admin") return <Navigate to="/dashboard" replace />;
  return children;
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<ProtectedRoute><HomePage /></ProtectedRoute>} />
      <Route path="/dashboard" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
      <Route path="/admin" element={<AdminRoute><AdminPage /></AdminRoute>} />
      <Route path="/login" element={<GuestRoute><LoginPage /></GuestRoute>} />
      <Route path="/register" element={<GuestRoute><RegisterPage /></GuestRoute>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
