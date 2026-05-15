import apiClient from './client'

export const NOTIFICATIONS_CHANGED_EVENT = 'learnly:notifications-changed'

export const notifyNotificationsChanged = () => {
  window.dispatchEvent(new CustomEvent(NOTIFICATIONS_CHANGED_EVENT))
}

export const fetchNotifications = async () => {
  const { data } = await apiClient.get('/api/notifications')
  return data
}

export const markNotificationAsRead = async (notificationId) => {
  await apiClient.patch(`/api/notifications/${notificationId}/read`)
  notifyNotificationsChanged()
}
