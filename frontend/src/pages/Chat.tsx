import React from 'react';
import { useNavigate } from 'react-router-dom';
import Chatbot from '../components/Chatbot';

const Chat: React.FC = () => {
  const navigate = useNavigate();

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

      <main className="flex-1 flex items-stretch min-h-0">
        <div className="flex-1 max-w-2xl mx-auto bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden flex flex-col h-full">
          <Chatbot />
        </div>
      </main>
    </div>
  );
};

export default Chat;
