import api from './api';

export interface ChatbotSource {
  title: string;
  slug: string;
}

export interface ChatbotResponse {
  message: string;
  sources: ChatbotSource[];
}

export const sendMessageToBot = async (message: string): Promise<ChatbotResponse> => {
  const response = await api.post<ChatbotResponse>('/chatbot', { message });
  return response.data;
};
