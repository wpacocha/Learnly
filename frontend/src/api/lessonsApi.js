import apiClient from './client'

export const fetchMyLessons = async () => {
  const { data } = await apiClient.get('/api/lessons/mine')
  return data
}

export const bookLesson = async (tutorAvailabilitySlotId) => {
  const { data } = await apiClient.post('/api/lessons/book', { tutorAvailabilitySlotId })
  return data
}

export const updateLessonStatus = async (lessonId, status) => {
  const { data } = await apiClient.patch(`/api/lessons/${lessonId}/status`, null, {
    params: { status },
  })
  return data
}
