import { useNavigate } from 'react-router-dom';

const AdminStats = () => {
  const navigate = useNavigate();

  return (
    <div className="container mt-4">
      {/* 🔙 Back to Dashboard */}
      <button
        className="btn btn-outline-secondary mb-3"
        onClick={() => navigate('/dashboard')}
      >
        ← Back to Dashboard
      </button>

      <div className="p-6">
        <h1 className="text-2xl font-bold mb-2">Admin Dashboard</h1>
        <p className="text-gray-600">
          System health and usage stats will appear here.
        </p>
      </div>
    </div>
  );
};

export default AdminStats;
