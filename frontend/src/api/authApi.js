import apiClient from './client'

export const registerUser = async (payload) => {
  const { data } = await apiClient.post('/api/auth/register', payload)
  return data
}

export const loginUser = async (payload) => {
  const { data } = await apiClient.post('/api/auth/login', payload)
  return data
}

export const getCurrentUser = async () => {
  const { data } = await apiClient.get('/api/auth/me')
  return data
}
