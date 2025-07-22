import React, { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const DocumentView: React.FC = () => {
  const { fileName } = useParams();
  const { user, loading } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!loading && !user) {
      navigate('/login');
    }
  }, [loading, user, navigate]);

  if (loading || !user) return <div className="container mt-4">Loading...</div>;

  return (
    <div className="container mt-4">
      <button
        className="btn btn-outline-secondary mb-3"
        onClick={() => navigate('/documents')}
      >
        ← Back to Document Library
      </button>

      <h2 className="mb-3">{fileName}</h2>

      <iframe
        src={`https://moai-backend-gbgv.onrender.com/api/document/view/${encodeURIComponent(fileName!)}`}
        title="Document Viewer"
        width="100%"
        height="600px"
        style={{ border: '1px solid #ccc' }}
      />
    </div>
  );
};

export default DocumentView;
