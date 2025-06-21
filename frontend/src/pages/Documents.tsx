import React, { useEffect, useState } from 'react';
import { getAllDocuments, deleteDocument } from '../services/api';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

// 🔧 Type definition for a stored document
interface Document {
  id: number;
  fileName: string;
  department: string;
  uploadedBy: string;
  uploadedAt: string;
}

const Documents: React.FC = () => {
  const [docs, setDocs] = useState<Document[]>([]);
  const [filteredDocs, setFilteredDocs] = useState<Document[]>([]);
  const [departments, setDepartments] = useState<string[]>([]);
  const [selectedDept, setSelectedDept] = useState<string>('All');

  const [currentPage, setCurrentPage] = useState(1);
  const docsPerPage = 5;

  const navigate = useNavigate();
  const { user } = useAuth();

  // 🔃 Fetch documents from backend
  useEffect(() => {
    getAllDocuments()
      .then((data) => {
        setDocs(data);
        setFilteredDocs(data);
        const uniqueDepts = Array.from(new Set(data.map((d) => d.department)));
        setDepartments(['All', ...uniqueDepts]);
      })
      .catch((err) => console.error('Failed to fetch documents:', err));
  }, []);

  // 🎯 Filter documents when department is changed
  useEffect(() => {
    const filtered =
      selectedDept === 'All'
        ? docs
        : docs.filter((doc) => doc.department === selectedDept);
    setFilteredDocs(filtered);
    setCurrentPage(1); // Reset to page 1 when filter changes
  }, [selectedDept, docs]);

  // 🧮 Pagination calculations
  const indexOfLastDoc = currentPage * docsPerPage;
  const indexOfFirstDoc = indexOfLastDoc - docsPerPage;
  const currentDocs = filteredDocs.slice(indexOfFirstDoc, indexOfLastDoc);
  const totalPages = Math.ceil(filteredDocs.length / docsPerPage);

  // 🗑️ Delete document handle
  const handleDelete = async (fileName: string) => {
    if (!window.confirm(`Delete ${fileName}?`)) return;
    try {
      await deleteDocument(fileName);
      setDocs((prev) => prev.filter((doc) => doc.fileName !== fileName));
    } catch {
      alert('Failed to delete document.');
    }
  };

  return (
    <div className="container mt-4">
      {/* 🔙 Back to Dashboard */}
      <button
        className="btn btn-outline-secondary mb-3"
        onClick={() => navigate('/dashboard')}
      >
        ← Back to Dashboard
      </button>

      <h2 className="mb-4">Document Library 📄</h2>

      {/* 🧭 Department Filter */}
      <div className="mb-3">
        <label className="form-label">Filter by Department:</label>
        <select
          className="form-select"
          value={selectedDept}
          onChange={(e) => setSelectedDept(e.target.value)}
        >
          {departments.map((dept) => (
            <option key={dept} value={dept}>
              {dept}
            </option>
          ))}
        </select>
      </div>

      {/* ⚠️ No documents found */}
      {filteredDocs.length === 0 ? (
        <div className="alert alert-warning">No documents available.</div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="table table-striped table-bordered align-middle">
              <thead className="table-light">
                <tr>
                  <th>File Name</th>
                  <th className="text-center">Department</th>
                  <th>Uploaded By</th>
                  <th>Uploaded At</th>
                  <th>View</th>
                </tr>
              </thead>
              <tbody>
                {currentDocs.map((doc) => (
                  <tr key={doc.id}>
                    <td>{doc.fileName}</td>
                    <td className="text-center">{doc.department}</td>
                    <td>{doc.uploadedBy}</td>
                    <td>{new Date(doc.uploadedAt).toLocaleString()}</td>
                    <td>
                      <button
                        className="btn btn-sm btn-outline-primary"
                        onClick={() =>
                          navigate(
                            `/documents/${encodeURIComponent(doc.fileName)}`
                          )
                        }
                      >
                        View
                      </button>
                      {['Admin', 'FullTime'].includes(user?.role || '') && (
                        <button
                          className="btn btn-sm btn-outline-danger ms-2"
                          onClick={() => handleDelete(doc.fileName)}
                        >
                          Delete
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* ⏩ Pagination */}
          <div className="d-flex justify-content-between align-items-center mt-3">
            <button
              className="btn btn-outline-primary"
              onClick={() => setCurrentPage((p) => p - 1)}
              disabled={currentPage === 1}
            >
              ⬅ Previous
            </button>
            <span>
              Page {currentPage} of {totalPages}
            </span>
            <button
              className="btn btn-outline-primary"
              onClick={() => setCurrentPage((p) => p + 1)}
              disabled={currentPage === totalPages}
            >
              Next ➡
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default Documents;
