import apiClient from './client'

export const fetchMyAvailabilitySlots = async () => {
  const { data } = await apiClient.get('/api/tutor-availability')
  return data
}

export const createAvailabilitySlot = async (payload) => {
  const { data } = await apiClient.post('/api/tutor-availability', payload)
  return data
}

export const deleteAvailabilitySlot = async (slotId) => {
  await apiClient.delete(`/api/tutor-availability/${slotId}`)
}
