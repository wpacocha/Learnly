import apiClient from './client'

export const getTutorProfile = async () => {
  const response = await apiClient.get('/api/tutor-profile', {
    validateStatus: (status) => status === 200 || status === 404,
  })

  if (response.status === 404) {
    return null
  }

  return response.data
}

export const createTutorProfile = async (payload) => {
  const { data } = await apiClient.post('/api/tutor-profile', payload)
  return data
}

export const updateTutorProfile = async (payload) => {
  const { data } = await apiClient.put('/api/tutor-profile', payload)
  return data
}
