import axios from 'axios';
import type { StoredDocument, UploadResponse } from '../types';
import type { LoginResponse } from '../shared/types';

// 📦 Base API client with auto-token support
const API_BASE = 'https://localhost:5000/api';

export const apiClient = axios.create({
  baseURL: API_BASE,
});

// 🔐 Inject token into every request if available
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token && config.headers) {
    config.headers['Authorization'] = `Bearer ${token}`;
  }

  return config;
});

// 🔐 Login and return token + user info
export const loginUser = async (
  email: string,
  password: string
): Promise<LoginResponse> => {
  const res = await apiClient.post<LoginResponse>('/auth/login', {
    email,
    password,
  });
  return res.data;
};

// 📤 Upload a document (txt/md/docx)
export const uploadDocument = async (file: File): Promise<UploadResponse> => {
  const formData = new FormData();
  formData.append('file', file);

  const res = await apiClient.post<UploadResponse>(
    '/document/upload',
    formData
  );
  return res.data;
};

// 📄 Get all stored documents (filtered by role/department on server)
export const getAllDocuments = async (): Promise<StoredDocument[]> => {
  const res = await apiClient.get<StoredDocument[]>('/document');
  return res.data;
};

export const getDocumentByName = async (
  fileName: string
): Promise<StoredDocument> => {
  const res = await apiClient.get<StoredDocument>(
    `/document/${encodeURIComponent(fileName)}`
  );
  return res.data;
};

// 🗑️ Delete a document by filename
export const deleteDocument = async (fileName: string): Promise<void> => {
  await apiClient.delete(`/document/delete/${encodeURIComponent(fileName)}`);
};
