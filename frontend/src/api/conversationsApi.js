import apiClient from './client'

export const fetchConversations = async () => {
  const { data } = await apiClient.get('/api/conversations')
  return data
}

export const startConversation = async (tutorProfileId) => {
  const { data } = await apiClient.post('/api/conversations', { tutorProfileId })
  return data
}

export const fetchConversationMessages = async (conversationId) => {
  const { data } = await apiClient.get(`/api/conversations/${conversationId}/messages`)
  return data
}

export const sendConversationMessage = async (conversationId, message) => {
  const { data } = await apiClient.post(`/api/conversations/${conversationId}/messages`, { message })
  return data
}
