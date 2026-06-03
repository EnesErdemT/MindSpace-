'use client';

import React, { useState, useRef, useEffect } from 'react';
import Link from 'next/link';
import { MessageCircle, X, Send, Bot, Sparkles, RefreshCw } from 'lucide-react';
import { sendMessageToBot, ChatbotSource } from '@/services/chatbot';

interface Message {
  id: string;
  text: string;
  sender: 'user' | 'bot';
  timestamp: Date;
  sources?: ChatbotSource[];
}

export default function Chatbot() {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([
    {
      id: 'welcome',
      text: 'Bună! Sunt asistentul tău virtual MindSpace AI. Îți pot oferi detalii despre articolele noastre sau răspunde la întrebări. Despre ce vrei să discutăm?',
      sender: 'bot',
      timestamp: new Date()
    }
  ]);
  const [inputValue, setInputValue] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const quickPrompts = [
    'Ce este .NET 9?',
    'Orchestrare Kubernetes',
    'Securitate Cloud',
    'AI și Etică'
  ];

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    if (isOpen) {
      scrollToBottom();
    }
  }, [messages, isOpen]);

  const handleSend = async (text: string) => {
    if (!text.trim()) return;

    const userMessage: Message = {
      id: `user-${Date.now()}`,
      text,
      sender: 'user',
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setInputValue('');
    setIsLoading(true);

    try {
      const response = await sendMessageToBot(text);
      
      const botMessage: Message = {
        id: `bot-${Date.now()}`,
        text: response.message,
        sender: 'bot',
        timestamp: new Date(),
        sources: response.sources
      };

      setMessages(prev => [...prev, botMessage]);
    } catch (error) {
      console.error('Error getting chatbot response:', error);
      const errorMessage: Message = {
        id: `error-${Date.now()}`,
        text: 'Ne pare rău, a apărut o problemă la conexiunea cu asistentul AI. Te rog să încerci din nou.',
        sender: 'bot',
        timestamp: new Date()
      };
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSend(inputValue);
    }
  };

  // Un parser simplu pentru Markdown (bold, link-uri, bullets, newlines)
  const parseMarkdown = (text: string) => {
    const lines = text.split('\n');
    return lines.map((line, lineIdx) => {
      // Verifică listă cu bullet
      const isBullet = line.trim().startsWith('* ') || line.trim().startsWith('- ');
      let content = isBullet ? line.trim().substring(2) : line;

      // Parsează bold: **text**
      const boldRegex = /\*\*([^*]+)\*\*/g;
      const parts: React.ReactNode[] = [];
      let lastIndex = 0;
      let match;

      // Parsează link-uri: [Titlu](url)
      const linkRegex = /\[([^\]]+)\]\(([^)]+)\)/g;

      // Combinăm regex-urile sau facem înlocuiri simple
      // Pentru simplitate, folosim înlocuire text la nivel de bucăți
      // O abordare mai robustă în React:
      let tempText = content;
      
      // Procesăm mai întâi link-urile
      const elements: React.ReactNode[] = [];
      let currentIdx = 0;

      const combinedRegex = /(\*\*([^*]+)\*\*)|(\[([^\]]+)\]\(([^)]+)\))/g;
      
      while ((match = combinedRegex.exec(tempText)) !== null) {
        // Adăugăm textul dinainte de potrivire
        if (match.index > currentIdx) {
          elements.push(tempText.substring(currentIdx, match.index));
        }

        if (match[1]) {
          // Bold matches
          elements.push(<strong key={match.index} className="font-bold text-gray-900">{match[2]}</strong>);
        } else if (match[3]) {
          // Link matches
          const isInternal = match[5].startsWith('/');
          if (isInternal) {
            elements.push(
              <Link key={match.index} href={match[5]} className="text-violet-600 hover:text-violet-800 underline font-medium">
                {match[4]}
              </Link>
            );
          } else {
            elements.push(
              <a key={match.index} href={match[5]} target="_blank" rel="noopener noreferrer" className="text-violet-600 hover:text-violet-800 underline font-medium">
                {match[4]}
              </a>
            );
          }
        }

        currentIdx = combinedRegex.lastIndex;
      }

      if (currentIdx < tempText.length) {
        elements.push(tempText.substring(currentIdx));
      }

      const renderedContent = elements.length > 0 ? elements : content;

      if (isBullet) {
        return (
          <li key={lineIdx} className="ml-4 list-disc mb-1 text-sm text-gray-700">
            {renderedContent}
          </li>
        );
      }

      return (
        <p key={lineIdx} className="mb-2 text-sm leading-relaxed text-gray-700 min-h-[1rem]">
          {renderedContent}
        </p>
      );
    });
  };

  return (
    <div className="fixed bottom-6 right-6 z-50 font-sans">
      {/* Fereastra de Chat */}
      {isOpen && (
        <div className="absolute bottom-16 right-0 w-[380px] sm:w-[400px] h-[550px] bg-white/95 backdrop-blur-md rounded-2xl border border-gray-200 shadow-2xl flex flex-col overflow-hidden transition-all duration-300 transform scale-100 origin-bottom-right">
          {/* Header */}
          <div className="bg-gradient-to-r from-violet-600 to-indigo-600 p-4 text-white flex justify-between items-center shadow-md">
            <div className="flex items-center space-x-3">
              <div className="relative bg-white/10 p-2 rounded-lg">
                <Bot className="h-5 w-5 text-white" />
                <span className="absolute bottom-0 right-0 w-2.5 h-2.5 bg-green-400 border-2 border-indigo-600 rounded-full animate-pulse"></span>
              </div>
              <div>
                <h3 className="font-semibold text-sm">MindSpace AI</h3>
                <span className="text-xs text-violet-100 flex items-center">
                  <Sparkles className="h-3 w-3 mr-1" />
                  Asistent Virtual Inteligent
                </span>
              </div>
            </div>
            <button 
              onClick={() => setIsOpen(false)}
              className="text-white/80 hover:text-white p-1 rounded-full hover:bg-white/10 transition-colors"
            >
              <X className="h-5 w-5" />
            </button>
          </div>

          {/* Mesaje */}
          <div className="flex-1 p-4 overflow-y-auto space-y-4 bg-gray-50/50">
            {messages.map((msg) => (
              <div 
                key={msg.id}
                className={`flex ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}
              >
                <div className={`max-w-[85%] rounded-2xl px-4 py-2.5 shadow-sm ${
                  msg.sender === 'user'
                    ? 'bg-violet-600 text-white rounded-br-none'
                    : 'bg-white text-gray-800 border border-gray-100 rounded-bl-none'
                }`}>
                  <div className={msg.sender === 'user' ? 'text-white' : 'text-gray-800'}>
                    {msg.sender === 'user' ? msg.text : parseMarkdown(msg.text)}
                  </div>
                  
                  {/* Afișare Surse */}
                  {msg.sources && msg.sources.length > 0 && (
                    <div className="mt-3 pt-2 border-t border-gray-100 text-xs text-gray-500">
                      <span className="font-semibold block mb-1">Surse de referință:</span>
                      <div className="space-y-1">
                        {msg.sources.map((src, idx) => (
                          <Link 
                            key={idx} 
                            href={`/post/${src.slug}`}
                            className="text-violet-600 hover:underline block truncate"
                          >
                            📖 {src.title}
                          </Link>
                        ))}
                      </div>
                    </div>
                  )}
                  
                  <span className={`text-[10px] block text-right mt-1.5 ${
                    msg.sender === 'user' ? 'text-violet-200' : 'text-gray-400'
                  }`}>
                    {msg.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </span>
                </div>
              </div>
            ))}
            
            {/* Loading Indicator */}
            {isLoading && (
              <div className="flex justify-start">
                <div className="bg-white border border-gray-100 rounded-2xl rounded-bl-none px-4 py-3 shadow-sm flex items-center space-x-2">
                  <div className="flex space-x-1">
                    <span className="w-2 h-2 bg-violet-600 rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></span>
                    <span className="w-2 h-2 bg-violet-600 rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></span>
                    <span className="w-2 h-2 bg-violet-600 rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></span>
                  </div>
                  <span className="text-xs text-gray-500">MindSpace AI analizează...</span>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Quick suggestions when messages count is low */}
          {messages.length === 1 && (
            <div className="p-3 bg-gray-50 border-t border-gray-100">
              <span className="text-[11px] font-semibold text-gray-400 block mb-2 px-1">Întrebări Frecvente:</span>
              <div className="flex flex-wrap gap-1.5">
                {quickPrompts.map((prompt, idx) => (
                  <button
                    key={idx}
                    onClick={() => handleSend(prompt)}
                    className="text-xs bg-white hover:bg-violet-50 text-gray-600 hover:text-violet-700 border border-gray-200 hover:border-violet-300 rounded-full px-3 py-1 transition-all shadow-xs"
                  >
                    {prompt}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Input Panel */}
          <div className="p-3 bg-white border-t border-gray-100 flex items-center space-x-2">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyDown={handleKeyPress}
              placeholder="Scrie mesajul tău aici..."
              disabled={isLoading}
              className="flex-1 bg-gray-50 border border-gray-200 hover:border-gray-300 focus:border-violet-500 rounded-xl px-4 py-2 text-sm outline-none transition-all disabled:opacity-50 text-gray-800"
            />
            <button
              onClick={() => handleSend(inputValue)}
              disabled={!inputValue.trim() || isLoading}
              className="bg-violet-600 hover:bg-violet-700 disabled:bg-gray-200 text-white p-2 rounded-xl transition-all disabled:text-gray-400 shadow-sm hover:shadow-md active:scale-95"
            >
              <Send className="h-4.5 w-4.5" />
            </button>
          </div>
        </div>
      )}

      {/* Butonul Plutitor Principal (Chat Bubble) */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="bg-gradient-to-r from-violet-600 to-indigo-600 text-white p-4 rounded-full shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:scale-110 active:scale-95 group relative flex items-center justify-center"
        aria-label="Deschide Chatbot"
      >
        {isOpen ? (
          <X className="h-6 w-6 transition-transform duration-300 rotate-90" />
        ) : (
          <>
            <MessageCircle className="h-6 w-6 transition-transform duration-300 group-hover:rotate-12" />
            <span className="absolute -top-1 -right-1 flex h-3 w-3">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-violet-400 opacity-75"></span>
              <span className="relative inline-flex rounded-full h-3 w-3 bg-violet-500"></span>
            </span>
          </>
        )}
      </button>
    </div>
  );
}
