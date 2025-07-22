import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Chatbot from '../components/Chatbot';
import ChatSidebar from '../components/ChatSidebar';

const Chat: React.FC = () => {
  const navigate = useNavigate();
  const [chatSessionId, setChatSessionId] = useState<number | null>(null);
  const [_, setMessageCount] = useState(0);

  return (
    <div className="flex flex-col h-screen bg-gray-50 dark:bg-gray-900">
      <header className="flex items-center px-6 py-4 bg-white dark:bg-gray-800 shadow">
        <button
          className="text-gray-600 dark:text-gray-300 hover:underline"
          onClick={() => navigate('/dashboard')}
        >
          ← Back to Dashboard
        </button>
        <h1 className="flex-1 text-center text-xl font-semibold text-gray-800 dark:text-gray-100">
          Chat Assistant
        </h1>
        <div style={{ width: 96 }} />
      </header>

      <main className="flex-1 flex min-h-0">
        {/* 🧭 Sidebar */}
        <ChatSidebar
          selectedId={chatSessionId}
          onSelect={(id) => {
            setChatSessionId(id);
            setMessageCount(0);
          }}
        />

        {/* 💬 Chat window */}
        <div className="flex-1 max-w-2xl mx-auto bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden flex flex-col h-full">
          {chatSessionId ? (
            <Chatbot
              chatSessionId={chatSessionId}
              setChatSessionId={setChatSessionId}
              onMessageSent={() => setMessageCount((c) => c + 1)}
            />
          ) : (
            <div className="flex-1 flex items-center justify-center text-gray-500 p-4">
              Select or start a chat to begin
            </div>
          )}
        </div>
      </main>
    </div>
  );
};

export default Chat;
