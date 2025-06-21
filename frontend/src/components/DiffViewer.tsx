import React from 'react';

interface DiffViewerProps {
  oldText: string;
  newText: string;
  fileName: string;
}

const DiffViewer: React.FC<DiffViewerProps> = ({
  oldText,
  newText,
  fileName,
}) => {
  return (
    <div className="container mt-3">
      <h6>Comparing: {fileName}</h6>
      <div
        style={{
          display: 'flex',
          gap: '1rem',
          flexWrap: 'wrap',
          justifyContent: 'center',
        }}
      >
        <div style={{ flex: 1, minWidth: '300px' }}>
          <strong>Existing:</strong>
          <pre
            className="border p-2 bg-light overflow-auto"
            style={{ maxHeight: '400px', whiteSpace: 'pre-wrap' }}
          >
            {oldText}
          </pre>
        </div>
        <div style={{ flex: 1, minWidth: '300px' }}>
          <strong>Uploaded:</strong>
          <pre
            className="border p-2 bg-light overflow-auto"
            style={{ maxHeight: '400px', whiteSpace: 'pre-wrap' }}
          >
            {newText}
          </pre>
        </div>
      </div>
    </div>
  );
};

export default DiffViewer;
