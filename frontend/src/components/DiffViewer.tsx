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
    <div>
      <h6>Comparing: {fileName}</h6>
      <div className="row">
        <div className="col">
          <strong>Existing:</strong>
          <pre className="border p-2 bg-light">{oldText}</pre>
        </div>
        <div className="col">
          <strong>Uploaded:</strong>
          <pre className="border p-2 bg-light">{newText}</pre>
        </div>
      </div>
    </div>
  );
};

export default DiffViewer;
