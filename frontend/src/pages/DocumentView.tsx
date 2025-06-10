import React, { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { deleteDocument } from '../services/api';

const DocumentView: React.FC = () => {
  const { fileName } = useParams();
  const { user, loading } = useAuth();
  const navigate = useNavigate();

  // 🛡 Prevent redirect until loading completes
  useEffect(() => {
    if (loading) return;
    if (!user) navigate('/login');
  }, [loading, user, navigate]);

  const handleDelete = async () => {
    if (!fileName) return;
    try {
      await deleteDocument(fileName);
      navigate('/documents');
    } catch (error) {
      alert('Failed to delete document.');
    }
  };

  // 🕒 Show loading state instead of null
  if (loading) return <div className="container mt-4">Loading...</div>;
  if (!user) return null;

  return (
    <div className="container mt-4">
      <h2>{fileName}</h2>
      <iframe
        src={`/api/document/view/${encodeURIComponent(fileName!)}`}
        title="Document Preview"
        width="100%"
        height="600px"
      />

      {user.role === 'Admin' && (
        <button className="btn btn-danger mt-3" onClick={handleDelete}>
          Delete Document
        </button>
      )}
    </div>
  );
};

export default DocumentView;
