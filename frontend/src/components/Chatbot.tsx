import React, { useState, useRef, useEffect } from 'react';

const Chatbot: React.FC = () => {
  const [messages, setMessages] = useState<
    { sender: 'user' | 'bot'; text: string }[]
  >([]);
  const [input, setInput] = useState('');
  const [isStreaming, setIsStreaming] = useState(false);
  const endRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Auto-expand textarea height
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = `${textareaRef.current.scrollHeight}px`;
    }
  }, [input]);

  // Scroll to latest message
  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim() || isStreaming) return;
    setMessages((m) => [...m, { sender: 'user', text: input }]);
    setInput('');
    setIsStreaming(true);

    const res = await fetch('/api/chat?stream=true', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message: input }),
    });

    if (!res.ok) {
      const err = await res.text();
      setMessages((m) => [...m, { sender: 'bot', text: `Error: ${err}` }]);
      setIsStreaming(false);
      return;
    }

    setMessages((m) => [...m, { sender: 'bot', text: '' }]);
    const reader = res.body!.getReader();
    const dec = new TextDecoder();
    let buf = '';

    while (true) {
      const { value, done } = await reader.read();
      if (done) break;
      buf += dec.decode(value);
      setMessages((m) => {
        const copy = [...m];
        copy[copy.length - 1] = { sender: 'bot', text: buf };
        return copy;
      });
    }

    setIsStreaming(false);
  };

  return (
    <div className="flex flex-col flex-1 min-h-0">
      {/* Chat area */}
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
            </div>
          </div>
        ))}
        <div ref={endRef} />
      </div>

      {/* Input bar */}
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
