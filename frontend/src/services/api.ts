import axios from 'axios';
import type { StoredDocument, UploadResponse } from '../types';
import type { LoginResponse, ChatMessage } from '../shared/types';
import type { SystemStats } from '../types';

// 📦 Base API client with cookied-based auth support
export const API_BASE = 'https://moai-backend-gbgv.onrender.com/api';

export const apiClient = axios.create({
  baseURL: API_BASE,
  withCredentials: true, // ✅ Ensures cookies are sent with each request
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

// Chat
export const streamChat = async (
  chatSessionId: number,
  message: string
): Promise<ReadableStream<Uint8Array> | null> => {
  const res = await fetch(`${API_BASE}/chat/send`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify({ message, chatSessionId }),
  });

  if (!res.ok) {
    const err = await res.text();
    throw new Error(`Chat failed: ${res.status} ${err}`);
  }

  return res.body;
};

// 🛠️ Get system stats (admin only)
export const getSystemStats = async (): Promise<SystemStats> => {
  const res = await apiClient.get<SystemStats>('/admin/stats');
  return res.data;
};

// 💬 Get all chat sessions for current user
export const getChats = async (): Promise<ChatSession[]> => {
  const res = await apiClient.get<ChatSession[]>('/chat');
  return res.data;
};

// ➕ Create a new chat session (optional title)
export const createChat = async (title: string = ''): Promise<ChatSession> => {
  const res = await apiClient.post<ChatSession>('/chat/new', title, {
    headers: { 'Content-Type': 'application/json' },
  });
  return res.data;
};

// 🗑️ Delete a chat session by ID
export const deleteChat = async (id: number): Promise<void> => {
  await apiClient.delete(`/chat/${id}`);
};

export const rateMessage = async (messageId: number, isHelpful: boolean) => {
  await apiClient.post('/chat/rate', { messageId, isHelpful });
};

export interface ChatSession {
  id: number;
  title: string;
  createdAt: string;
}

// api call to fetch message for a chat session
export const getMessages = async (chatId: number): Promise<ChatMessage[]> => {
  const res = await apiClient.get<ChatMessage[]>(`/chat/${chatId}/messages`);
  return res.data;
};
