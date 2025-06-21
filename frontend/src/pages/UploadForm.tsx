import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import DiffViewer from '../components/DiffViewer';
import { getDocumentByName, apiClient } from '../services/api';

const UploadForm = () => {
  const [file, setFile] = useState<File | null>(null);
  const [existingText, setExistingText] = useState('');
  const [uploadedText, setUploadedText] = useState('');
  const [isDuplicate, setIsDuplicate] = useState(false);
  const [fileName, setFileName] = useState('');
  const [uploading, setUploading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [isPdf, setIsPdf] = useState(false);

  const navigate = useNavigate();

  const readFileAsText = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = reject;
      reader.readAsText(file);
    });
  };

  const handleUpload = async () => {
    if (!file) return;
    setSuccessMessage('');
    setUploading(true);

    const name = file.name;
    setFileName(name);
    const extension = name.split('.').pop()?.toLowerCase();
    setIsPdf(extension === 'pdf');

    try {
      if (extension === 'txt' || extension === 'md') {
        const uploaded = await readFileAsText(file);
        setUploadedText(uploaded);

        const existingDoc = await getDocumentByName(name);
        setExistingText(existingDoc.content || '');
        setIsDuplicate(true); // Show diff viewer
      } else {
        // No diff — assume direct upload (pdf, docx, etc.)
        await confirmUpload();
      }
    } catch {
      // No existing doc or fallback
      await confirmUpload();
    } finally {
      setUploading(false);
    }
  };

  const confirmUpload = async () => {
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);
    setUploading(true);

    try {
      await apiClient.post('/document/upload', formData);
      setSuccessMessage('✅ Document uploaded successfully.');
      setIsDuplicate(false);
      setFile(null);
    } catch {
      alert('❌ Upload failed.');
    } finally {
      setUploading(false);
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

      <h3>Upload Document</h3>

      {successMessage && (
        <div className="alert alert-success">{successMessage}</div>
      )}

      <input
        type="file"
        className="form-control"
        onChange={(e) => setFile(e.target.files?.[0] || null)}
      />

      <button
        className="btn btn-primary mt-2 me-2"
        onClick={handleUpload}
        disabled={!file || uploading}
      >
        {uploading ? 'Uploading...' : 'Upload'}
      </button>

      {/* 🧾 PDF Preview */}
      {isPdf && file && (
        <div className="mt-4">
          <h5>PDF Preview</h5>
          <iframe
            src={URL.createObjectURL(file)}
            title="PDF Preview"
            width="100%"
            height="600px"
          />
        </div>
      )}

      {/* 🧠 Text diff viewer */}
      {isDuplicate && (
        <div className="mt-10">
          <h5>File with same name exists. Here's the comparison:</h5>
          <DiffViewer
            oldText={existingText}
            newText={uploadedText}
            fileName={fileName}
          />
          <button
            className="btn btn-success mt-2"
            onClick={confirmUpload}
            disabled={uploading}
          >
            {uploading ? 'Confirming...' : 'Confirm Upload Anyway'}
          </button>
        </div>
      )}
    </div>
  );
};

export default UploadForm;
