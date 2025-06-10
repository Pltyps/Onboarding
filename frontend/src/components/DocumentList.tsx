import React, { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import type { StoredDocument } from '../types';

interface DocumentListProps {
  documents: StoredDocument[];
  title?: string;
}

const DocumentList: React.FC<DocumentListProps> = ({ documents, title }) => {
  const navigate = useNavigate();

  const departments = useMemo(() => {
    const set = new Set(documents.map((d) => d.department));
    return Array.from(set).sort();
  }, [documents]);

  const [selectedDept, setSelectedDept] = useState<string>('All');
  const [currentPage, setCurrentPage] = useState<number>(1);
  const itemsPerPage = 5;

  const filteredDocs =
    selectedDept === 'All'
      ? documents
      : documents.filter((d) => d.department === selectedDept);

  const totalPages = Math.ceil(filteredDocs.length / itemsPerPage);
  const paginatedDocs = filteredDocs.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

  const handleDeptChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedDept(e.target.value);
    setCurrentPage(1);
  };

  return (
    <div className="container mt-4">
      {/* 🔙 Back Button */}
      <div className="d-flex justify-content-between align-items-center mb-3">
        <button
          onClick={() => navigate('/dashboard')}
          className="btn btn-outline-secondary"
        >
          ← Back to Dashboard
        </button>

        {/* Optional: Title aligned right */}
        {title && <h2 className="mb-0">{title}</h2>}
      </div>

      {/* 🔽 Department Filter */}
      <div className="mb-3 d-flex align-items-center gap-2">
        <label htmlFor="deptFilter" className="form-label mb-0">
          Filter by Department:
        </label>
        <select
          id="deptFilter"
          className="form-select w-auto"
          value={selectedDept}
          onChange={handleDeptChange}
        >
          <option value="All">All</option>
          {departments.map((dept) => (
            <option key={dept} value={dept}>
              {dept}
            </option>
          ))}
        </select>
      </div>

      {filteredDocs.length === 0 ? (
        <div className="alert alert-warning">No documents found.</div>
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
                {paginatedDocs.map((doc) => (
                  <tr key={doc.id}>
                    <td>{doc.fileName}</td>
                    <td className="text-center">{doc.department}</td>
                    <td>{doc.uploadedBy}</td>
                    <td>{new Date(doc.uploadedAt).toLocaleString()}</td>
                    <td>
                      <a
                        href={`/documents/${encodeURIComponent(doc.fileName)}`}
                        className="btn btn-sm btn-outline-primary"
                      >
                        View
                      </a>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* 📄 Pagination */}
          <nav className="mt-3 d-flex justify-content-center">
            <ul className="pagination">
              {[...Array(totalPages)].map((_, i) => (
                <li
                  key={i}
                  className={`page-item ${currentPage === i + 1 ? 'active' : ''}`}
                >
                  <button
                    className="page-link"
                    onClick={() => setCurrentPage(i + 1)}
                  >
                    {i + 1}
                  </button>
                </li>
              ))}
            </ul>
          </nav>
        </>
      )}
    </div>
  );
};

export default DocumentList;
