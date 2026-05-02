import apiClient from './client'

export const fetchSubjects = async () => {
  const { data } = await apiClient.get('/api/catalog/subjects')
  return data
}

export const fetchTeachingLevels = async () => {
  const { data } = await apiClient.get('/api/catalog/teaching-levels')
  return data
}
