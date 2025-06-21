import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getSystemStats } from '../services/api';
import type { SystemStats } from '../types';

const formatUptime = (minutes: number): string => {
  const d = Math.floor(minutes / 1440);
  const h = Math.floor((minutes % 1440) / 60);
  const m = Math.floor(minutes % 60);
  return `${d}d ${h}h ${m}m`;
};

const AdminStats: React.FC = () => {
  const [stats, setStats] = useState<SystemStats | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const load = async () => {
      try {
        const data = await getSystemStats();
        setStats(data);
      } catch (err) {
        console.error('Failed to load stats', err);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) return <p>Loading system stats...</p>;
  if (!stats) return <p>Could not fetch stats.</p>;

  return (
    <div className="container mt-4">
      {/* 🔙 Back to Dashboard */}
      <button
        className="btn btn-outline-secondary mb-3"
        onClick={() => navigate('/dashboard')}
      >
        ← Back to Dashboard
      </button>

      <h3 className="mb-4">🔧 System Diagnostics</h3>
      <div className="row g-4">
        <div className="col-md-4">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <h5 className="card-title">🖥️ Environment</h5>
              <p>
                <strong>{stats.environment}</strong>
              </p>
              <p className="text-muted">Server: {stats.machineName}</p>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <h5 className="card-title">💾 Memory Usage</h5>
              <p className="display-6">{stats.memoryUsageMB.toFixed(2)} MB</p>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <h5 className="card-title">⏱️ Uptime</h5>
              <p className="display-6">{formatUptime(stats.uptimeMinutes)}</p>
            </div>
          </div>
        </div>

        <div className="col-md-6">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <h5 className="card-title">👥 Active Users</h5>
              <ul className="list-group list-group-flush">
                {Object.entries(stats.activeUsers).map(([role, count]) => (
                  <li
                    key={role}
                    className="list-group-item d-flex justify-content-between"
                  >
                    <span>{role}</span>
                    <span className="fw-bold">{count}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>

        <div className="col-md-6">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <h5 className="card-title">📅 Server Time</h5>
              <p className="display-6">
                {new Date(stats.serverTime).toLocaleString()}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminStats;
