import React, { useEffect, useState } from 'react';
import { getChats, createChat, deleteChat } from '../services/api';

interface ChatSession {
  id: number;
  title: string;
  createdAt: string;
}

interface ChatSidebarProps {
  selectedId: number | null;
  onSelect: (id: number | null) => void;
}

const ChatSidebar: React.FC<ChatSidebarProps> = ({ selectedId, onSelect }) => {
  const [chats, setChats] = useState<ChatSession[]>([]);
  const [loading, setLoading] = useState(true);

  const loadChats = async () => {
    setLoading(true);
    try {
      const data = await getChats();
      setChats(data);
    } catch (err) {
      console.error('Failed to load chats', err);
    } finally {
      setLoading(false);
    }
  };

  const handleNewChat = async () => {
    try {
      const newChat = await createChat('');
      setChats((prev) => [newChat, ...prev]);
      onSelect(newChat.id);
    } catch (err) {
      console.error('Failed to create chat', err);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this chat?')) return;
    try {
      await deleteChat(id);
      setChats((prev) => prev.filter((c) => c.id !== id));
      if (selectedId === id) onSelect(null);
    } catch (err) {
      console.error('Failed to delete chat', err);
    }
  };

  useEffect(() => {
    loadChats();
  }, []);

  return (
    <aside className="w-64 border-r bg-gray-50 dark:bg-gray-900 overflow-y-auto">
      <div className="flex justify-between items-center px-4 py-3 border-b">
        <h2 className="text-lg font-semibold">Your Chats</h2>
        <button className="btn btn-sm btn-primary" onClick={handleNewChat}>
          ＋
        </button>
      </div>

      {loading ? (
        <p className="p-4 text-sm">Loading...</p>
      ) : chats.length === 0 ? (
        <p className="p-4 text-sm text-muted">No chats yet.</p>
      ) : (
        <ul className="divide-y">
          {chats.map((chat) => (
            <li
              key={chat.id}
              className={`flex justify-between items-center px-4 py-3 cursor-pointer hover:bg-blue-50 dark:hover:bg-gray-700 ${
                chat.id === selectedId
                  ? 'bg-blue-100 dark:bg-gray-800 font-bold'
                  : ''
              }`}
              onClick={() => onSelect(chat.id)}
            >
              <span className="truncate">{chat.title}</span>
              <button
                className="btn btn-sm btn-outline-danger"
                onClick={(e) => {
                  e.stopPropagation();
                  handleDelete(chat.id);
                }}
              >
                🗑️
              </button>
            </li>
          ))}
        </ul>
      )}
    </aside>
  );
};

export default ChatSidebar;
