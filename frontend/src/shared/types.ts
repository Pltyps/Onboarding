export interface LoginResponse {
  email: string;
  role: string;
  department: string;
}

export interface ChatMessage {
  sender: 'user' | 'bot';
  text: string;
  messageId?: number;
  isHelpful?: boolean | null;
}
