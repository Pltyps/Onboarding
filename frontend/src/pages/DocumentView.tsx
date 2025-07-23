import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import axios from 'axios';
import Linkify from 'linkify-react';

interface DocumentInfo {
  fileName: string;
  pdfPath?: string;
  content?: string;
}

const DocumentView: React.FC = () => {
  const { fileName } = useParams();
  const { user, loading } = useAuth();
  const navigate = useNavigate();
  const [doc, setDoc] = useState<DocumentInfo | null>(null);

  useEffect(() => {
    if (!loading && !user) {
      navigate('/login');
    }
  }, [loading, user, navigate]);

  useEffect(() => {
    const fetchDocument = async () => {
      try {
        const res = await axios.get<DocumentInfo>(
          `/api/document/${encodeURIComponent(fileName!)}`
        );
        setDoc(res.data);
      } catch (err) {
        console.error('Failed to load document metadata:', err);
        setDoc(null); // explicitly ensure null if failure
      }
    };

    if (fileName && user) {
      fetchDocument();
    }
  }, [fileName, user]);

  if (loading || !user) return <div className="container mt-4">Loading...</div>;
  if (!doc)
    return (
      <div className="container mt-4 text-danger">
        Document not found or failed to load.
      </div>
    );

  return (
    <div className="container mt-4">
      <button
        className="btn btn-outline-secondary mb-3"
        onClick={() => navigate('/documents')}
      >
        ← Back to Document Library
      </button>

      <h2 className="mb-3">{doc.fileName}</h2>

      {doc.pdfPath ? (
        <iframe
          src={encodeURI(doc.pdfPath)}
          title="Document Viewer"
          width="100%"
          height="600px"
          style={{ border: '1px solid #ccc' }}
        />
      ) : doc.content ? (
        <div className="mb-3">
          <h4>Document Content:</h4>
          <div style={{ maxHeight: '600px', overflowY: 'auto' }}>
            <pre
              className="bg-light p-3 rounded"
              style={{ whiteSpace: 'pre-wrap' }}
            >
              <Linkify options={{ target: '_blank' }}>{doc.content}</Linkify>
            </pre>
          </div>
        </div>
      ) : (
        <p className="text-muted">Document preview not available.</p>
      )}
    </div>
  );
};

export default DocumentView;
