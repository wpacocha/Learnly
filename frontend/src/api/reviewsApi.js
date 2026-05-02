import apiClient from './client'

export const fetchReviewsForTutor = async (tutorProfileId) => {
  const { data } = await apiClient.get(`/api/reviews/tutor/${tutorProfileId}`)
  return data
}

export const createReview = async (payload) => {
  const { data } = await apiClient.post('/api/reviews', payload)
  return data
}
