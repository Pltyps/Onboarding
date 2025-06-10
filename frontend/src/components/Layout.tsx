import React from 'react';
import { useTheme } from '../context/ThemeContext';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { theme, toggleTheme } = useTheme();
  const { user, logout, loading } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (loading) return null; // Or a spinner

  return (
    <>
      {/* 🌗 Sticky themed navbar */}
      <nav className="navbar theme-header px-4 d-flex justify-content-between">
        <span className="navbar-brand mb-0 h4">
          BYU Marriott Onboarding Portal
        </span>
        Welcome {user?.email}
        <div className="d-flex gap-3 align-items-center">
          <button onClick={toggleTheme} className="btn btn-outline-secondary">
            {theme === 'light' ? '🌙 Dark Mode' : '☀️ Light Mode'}
          </button>
          <button onClick={handleLogout} className="btn btn-outline-danger">
            Log Out
          </button>
        </div>
      </nav>

      {/* 🌗 Themed main content */}
      <main className="theme-bg text-body flex-grow-1 py-4">
        <div className="container">{children}</div>
      </main>

      {/* 🌗 Themed footer */}
      <footer className="theme-header text-center py-3">
        <small>
          &copy; {new Date().getFullYear()} MOAI • Built with 🧠 + ❤️
        </small>
      </footer>
    </>
  );
};

export default Layout;
