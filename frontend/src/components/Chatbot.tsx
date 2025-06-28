import React, { useState, useRef, useEffect } from 'react';
import { rateMessage, getMessages } from '../services/api';

interface ChatbotProps {
  chatSessionId: number | null;
  setChatSessionId: (id: number) => void;
  onMessageSent: () => void;
}

const Chatbot: React.FC<ChatbotProps> = ({
  chatSessionId,
  setChatSessionId,
  onMessageSent,
}) => {
  interface ChatMessage {
    sender: 'user' | 'bot';
    text: string;
    messageId?: number;
    isHelpful?: boolean | null;
  }

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [isStreaming, setIsStreaming] = useState(false);
  const endRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Auto-expand textarea
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = `${textareaRef.current.scrollHeight}px`;
    }
  }, [input]);

  // Scroll to bottom on new message
  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Fetch messages when session changes
  useEffect(() => {
    if (chatSessionId === null) return;

    const loadMessages = async () => {
      try {
        const loaded = await getMessages(chatSessionId);
        setMessages(loaded);
      } catch (err) {
        console.error('Failed to load messages:', err);
      }
    };

    loadMessages();
  }, [chatSessionId]);

  const handleRating = async (index: number, isHelpful: boolean) => {
    const msg = messages[index];
    if (!msg.messageId || msg.isHelpful !== undefined) return;

    try {
      await rateMessage(msg.messageId, isHelpful);
      const updated = [...messages];
      updated[index] = { ...msg, isHelpful };
      setMessages(updated);
    } catch (err) {
      console.error('Rating failed', err);
    }
  };

  const sendMessage = async () => {
    if (!input.trim() || isStreaming) return;

    let sessionId = chatSessionId;

    if (!sessionId) {
      const res = await fetch('/api/chat/new', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(''),
      });

      const data = await res.json();
      const newId: number = data.id; // ✅ definitely a number
      setChatSessionId(newId);
      sessionId = newId;
    }

    setMessages((m) => [...m, { sender: 'user', text: input }]);
    setInput('');
    setIsStreaming(true);

    const res = await fetch('/api/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        message: input,
        chatSessionId: sessionId,
        isFirstMessage:
          messages.filter((m) => m.sender === 'user').length === 0,
      }),
    });

    if (!res.ok) {
      const err = await res.text();
      setMessages((m) => [...m, { sender: 'bot', text: `Error: ${err}` }]);
      setIsStreaming(false);
      return;
    }

    const { reply, messageId } = await res.json();
    setMessages((m) => [...m, { sender: 'bot', text: reply, messageId }]);
    setIsStreaming(false);
    onMessageSent();
  };

  return (
    <div className="flex flex-col flex-1 min-h-0">
      <div className="flex-1 overflow-y-auto px-6 py-4 flex flex-col gap-4">
        {messages.map((m, i) => (
          <div
            key={i}
            className={`flex ${m.sender === 'user' ? 'justify-end' : 'justify-start'}`}
          >
            <div
              className={`px-5 py-3 max-w-[80%] rounded-2xl shadow leading-relaxed whitespace-pre-wrap break-words transition-colors
                ${
                  m.sender === 'user'
                    ? 'bg-byuNavy text-white dark:bg-byuRoyal rounded-br-sm self-end'
                    : 'bg-gray-100 dark:bg-gray-700 text-black dark:text-white rounded-bl-sm self-start'
                }`}
            >
              {m.text}
              {m.sender === 'bot' && (
                <div className="mt-2 flex gap-2 text-sm text-gray-500">
                  <button
                    aria-label="Rate this message helpful"
                    onClick={() => handleRating(i, true)}
                    disabled={m.isHelpful !== undefined}
                    className={`hover:text-green-600 ${m.isHelpful === true ? 'font-bold text-green-600' : ''}`}
                  >
                    👍
                  </button>
                  <button
                    onClick={() => handleRating(i, false)}
                    disabled={m.isHelpful !== undefined}
                    className={`hover:text-red-600 ${m.isHelpful === false ? 'font-bold text-red-600' : ''}`}
                  >
                    👎
                  </button>
                </div>
              )}
            </div>
          </div>
        ))}
        {isStreaming && (
          <div className="flex justify-start">
            <div className="px-5 py-3 bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-300 rounded-2xl max-w-[80%] italic">
              ...
            </div>
          </div>
        )}
        <div ref={endRef} />
      </div>

      <form
        onSubmit={(e) => {
          e.preventDefault();
          sendMessage();
        }}
        className="flex items-end gap-2 px-6 py-4 bg-gray-50 dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700"
      >
        <div className="flex-1">
          <textarea
            ref={textareaRef}
            rows={1}
            className="w-full resize-none px-4 py-3 rounded-xl border border-gray-400 dark:border-gray-600 bg-white dark:bg-gray-700 text-black dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 text-base leading-snug max-h-[200px] overflow-hidden"
            placeholder={isStreaming ? 'Thinking…' : 'Type a message…'}
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
              }
            }}
            disabled={isStreaming}
          />
        </div>
        <button
          type="submit"
          disabled={!input.trim() || isStreaming}
          className="px-5 py-3 text-lg rounded-full bg-byuRoyal text-white hover:bg-blue-800 disabled:opacity-50 transition-colors"
        >
          ↵
        </button>
      </form>
    </div>
  );
};

export default Chatbot;
