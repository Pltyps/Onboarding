// 📦 Global shared types
// Used across components and services to maintain consistency

// ✅ Role type for access control
export type UserRole = 'Admin' | 'Student';

// 🧾 Document structure used in tables, previews, and API calls
export interface StoredDocument {
  id: number;
  fileName: string;
  department: string;
  content: string;
  uploadedBy: string;
  uploadedAt: string;
  approved?: boolean; // 🔧 Optional — used only for admin review
}

// 🔑 Minimal login response shape
export interface UserSession {
  email: string;
  role: UserRole;
}

// 📤 Upload API response when a duplicate is found
export interface UploadDuplicateResult {
  duplicate: true;
  fileName: string;
  existingContent: string;
  uploadedContent: string;
}

// ✅ Upload API response when no duplicate is found
export interface UploadSuccessResult {
  duplicate: false;
  message: string;
}

// 🔄 Combined result type
export type UploadResponse = UploadDuplicateResult | UploadSuccessResult;
