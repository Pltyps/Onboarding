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
    <div className="container mt-5" style={{ maxWidth: '500px' }}>
      <h2 className="mb-4">Login</h2>

      <img
        src="/byu-marriott-onboarding-logo.png"
        alt="BYU Marriott Logo"
        title="Go to login"
        style={{ maxWidth: '220px', cursor: 'pointer', marginBottom: '1rem' }}
        onClick={() => navigate('/login')}
      />

      <form onSubmit={handleSubmit}>
        <div className="form-group mb-3">
          <label>Email</label>
          <input
            type="email"
            className="form-control"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoFocus
          />
        </div>

        <div className="form-group mb-3">
          <label>Password</label>
          <input
            type="password"
            className="form-control"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        {error && <div className="alert alert-danger">{error}</div>}

        <button type="submit" className="btn btn-primary w-100">
          Sign In
        </button>
      </form>
    </div>
  );
};

export default Login;
