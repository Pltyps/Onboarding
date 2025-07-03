import React from 'react';
import { diff_match_patch, DIFF_INSERT, DIFF_DELETE } from 'diff-match-patch';

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
  const dmp = new diff_match_patch();
  const diffs = dmp.diff_main(oldText, newText);
  dmp.diff_cleanupSemantic(diffs);

  // Build highlighted uploaded text (with insert/delete markers)
  const renderNewContent = diffs.map(([op, text], idx) => {
    let style = {};
    if (op === DIFF_INSERT) {
      style = { backgroundColor: '#e6ffed' }; // green
    } else if (op === DIFF_DELETE) {
      style = {
        backgroundColor: '#ffeef0',
        textDecoration: 'line-through',
        opacity: 0.6,
      }; // red
    }

    return (
      <span key={idx} style={{ ...style, whiteSpace: 'pre-wrap' }}>
        {text}
      </span>
    );
  });

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
        {/* Left: Uploaded (highlighted) */}
        <div style={{ flex: 1, minWidth: '300px' }}>
          <strong>New Upload:</strong>
          <div
            className="border p-2 bg-light overflow-auto"
            style={{ maxHeight: '400px', fontFamily: 'monospace' }}
          >
            {renderNewContent}
          </div>
        </div>

        {/* Right: Existing stored version */}
        <div style={{ flex: 1, minWidth: '300px' }}>
          <strong>Existing:</strong>
          <pre
            className="border p-2 bg-light overflow-auto"
            style={{
              maxHeight: '400px',
              whiteSpace: 'pre-wrap',
              fontFamily: 'monospace',
            }}
          >
            {oldText}
          </pre>
        </div>
      </div>
    </div>
  );
};

export default DiffViewer;
