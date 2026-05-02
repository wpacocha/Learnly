import apiClient from './client'

export const searchTutors = async (params) => {
  const { data } = await apiClient.get('/api/tutors/search', { params })
  return data
}
