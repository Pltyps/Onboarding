import React, { useState } from 'react';
import { loginUser } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import type { UserRole } from '../types';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      const {
        email: userEmail,
        role,
        department,
      } = await loginUser(email, password);

      // ✅ Optionally store other user info
      localStorage.setItem(
        'user',
        JSON.stringify({ email: userEmail, role, department })
      );

      login(userEmail, role as UserRole, department); // if your useAuth expects department too

      navigate('/dashboard');
    } catch (err) {
      setError('Invalid credentials.');
      setPassword(''); // Optional: Clear password on error
    }
  };

  return (
    <div
      style={{
        backgroundColor: 'var(--byu-light)',
        minHeight: '100vh',
        minWidth: '100vw',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        padding: '1rem', // optional: improves mobile responsiveness
        boxSizing: 'border-box', // ensures padding doesn't break centering
      }}
    >
      <div
        className="shadow d-flex flex-column align-items-center justify-content-center p-4"
        style={{
          backgroundColor: 'var(--byu-royal)',
          borderRadius: '0.75rem',
          maxWidth: '420px',
          width: '100%',
          height: 'auto', // prevent overflow from breaking alignment
        }}
      >
        <h2 className="text-center mb-3" style={{ color: 'var(--byu-white)' }}>
          Login
        </h2>

        <img
          src="/BYUMarriott_centered_white-1.png"
          alt="BYU Marriott Logo"
          title="Go to login"
          onClick={() => navigate('/login')}
          style={{
            maxWidth: '220px',
            marginBottom: '1.5rem',
            cursor: 'pointer',
            display: 'block',
            marginLeft: 'auto',
            marginRight: 'auto',
          }}
        />

        <form
          onSubmit={handleSubmit}
          style={{
            width: '100%',
            textAlign: 'left',
            display: 'flex',
            flexDirection: 'column',
            gap: '1rem',
            alignItems: 'center',
          }}
        >
          {/* Email Row */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <label
              htmlFor="email"
              style={{
                color: 'var(--byu-white)',
                width: '70px',
                textAlign: 'right',
              }}
            >
              Email
            </label>
            <input
              id="email"
              type="email"
              className="form-control"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoFocus
              style={{
                backgroundColor: 'white',
                width: '220px',
              }}
            />
          </div>

          {/* Password Row */}
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              marginBottom: '1.5rem', // ← adds visual spacing before button
            }}
          >
            <label
              htmlFor="password"
              style={{
                color: 'var(--byu-white)',
                width: '70px',
                textAlign: 'right',
              }}
            >
              Password
            </label>
            <input
              id="password"
              type="password"
              className="form-control"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              style={{
                backgroundColor: 'white',
                width: '220px',
              }}
            />
          </div>

          {error && (
            <div className="alert alert-danger bg-danger-subtle text-danger text-center w-100">
              {error}
            </div>
          )}

          <button
            type="submit"
            className="btn"
            style={{
              backgroundColor: 'var(--byu-blue)',
              color: 'var(--byu-light)',
              fontWeight: 600,
              width: '90px', // ✅ matches input field width
              height: '25px',
              marginTop: '0.5rem',
              marginBottom: '1rem', // ✅ adds space before container bottom
            }}
          >
            Sign In
          </button>
        </form>
      </div>
    </div>
  );
};

export default Login;
