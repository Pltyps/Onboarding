import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Linkify from 'linkify-react';
import { getDocumentByName } from '../services/api';

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
  const apiBase =
    import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

  useEffect(() => {
    if (!loading && !user) {
      navigate('/login');
    }
  }, [loading, user, navigate]);

  useEffect(() => {
    const fetchDocument = async () => {
      try {
        const res = await getDocumentByName(fileName!);
        setDoc(res);
      } catch (err) {
        console.error('Failed to load document metadata:', err);
        setDoc(null);
      }
    };

    if (fileName && user) {
      fetchDocument();
    }
  }, [fileName, user]);

  const isValidPdfUrl = doc?.pdfPath?.startsWith('http');

  if (loading || !user) {
    return (
      <div className="container mt-4 text-muted">
        <div className="spinner-border spinner-border-sm me-2" role="status" />
        Loading document...
      </div>
    );
  }

  if (!doc) {
    return (
      <div className="container mt-4">
        <button
          className="btn btn-outline-secondary mb-3"
          onClick={() => navigate('/documents')}
        >
          ← Back to Document Library
        </button>
        <div className="text-danger">Document not found or failed to load.</div>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <button
        className="btn btn-outline-secondary mb-3"
        onClick={() => navigate('/documents')}
      >
        ← Back to Document Library
      </button>

      <h2 className="mb-3">{doc.fileName}</h2>

      {isValidPdfUrl ? (
        <>
          <iframe
            src={`${apiBase}/document/view/${encodeURIComponent(doc.fileName)}`}
            width="100%"
            height="600px"
            title="Document Viewer"
            style={{ border: '1px solid #ccc' }}
            loading="lazy"
          />

          <a
            href={`${apiBase}/document/view/${encodeURIComponent(doc.fileName)}`}
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-sm btn-outline-primary"
          >
            Open PDF in new tab
          </a>
        </>
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
