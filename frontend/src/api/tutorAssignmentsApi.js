import apiClient from './client'

export const fetchMyAssignments = async () => {
  const response = await apiClient.get('/api/tutor-assignments', {
    validateStatus: (s) => s === 200 || s === 404,
  })
  if (response.status === 404) {
    return null
  }
  return response.data
}

export const updateMyAssignments = async (payload) => {
  const { data } = await apiClient.put('/api/tutor-assignments', payload)
  return data
}
