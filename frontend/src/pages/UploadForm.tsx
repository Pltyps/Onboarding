import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import DiffViewer from '../components/DiffViewer';

interface UploadResponse {
  duplicate: boolean;
  existingContent?: string;
  uploadedContent?: string;
  fileName?: string;
}

const UploadForm = () => {
  const [file, setFile] = useState<File | null>(null);
  const [existingText, setExistingText] = useState('');
  const [uploadedText, setUploadedText] = useState('');
  const [isDuplicate, setIsDuplicate] = useState(false);
  const [fileName, setFileName] = useState('');

  const navigate = useNavigate();

  const handleUpload = async () => {
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    try {
      const res = await axios.post<UploadResponse>(
        'https://localhost:5000/api/document/upload',
        formData
      );

      if (res.data.duplicate) {
        setExistingText(res.data.existingContent || '');
        setUploadedText(res.data.uploadedContent || '');
        setFileName(res.data.fileName || '');
        setIsDuplicate(true);
      } else {
        alert('✅ File uploaded successfully!');
      }
    } catch (err: any) {
      alert('❌ Upload failed: ' + (err?.response?.data || err.message));
    }
  };

  const confirmUpload = async () => {
    setIsDuplicate(false);
    alert('✅ Changes confirmed and uploaded.');
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

      <h3>Upload Document</h3>
      <input
        type="file"
        className="form-control"
        onChange={(e) => setFile(e.target.files?.[0] || null)}
      />
      <button className="btn btn-primary mt-2 me-2" onClick={handleUpload}>
        Upload
      </button>

      {isDuplicate && (
        <div className="mt-4">
          <h5>File with same name exists. Here's the comparison:</h5>
          <DiffViewer
            oldText={existingText}
            newText={uploadedText}
            fileName={fileName}
          />
          <button className="btn btn-success mt-2" onClick={confirmUpload}>
            Confirm Upload Anyway
          </button>
        </div>
      )}
    </div>
  );
};

export default UploadForm;
