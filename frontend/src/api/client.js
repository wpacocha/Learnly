import axios from 'axios'

export const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5159'
const TOKEN_KEY = 'learnly_token'

/** Pełny URL do zdjęć z API (ścieżki względne z uploadu) lub zewnętrznych linków. */
export const resolveMediaUrl = (pathOrUrl) => {
  if (!pathOrUrl) {
    return null
  }
  if (
    pathOrUrl.startsWith('http://') ||
    pathOrUrl.startsWith('https://') ||
    pathOrUrl.startsWith('data:')
  ) {
    return pathOrUrl
  }
  const base = API_BASE_URL.replace(/\/$/, '')
  const path = pathOrUrl.startsWith('/') ? pathOrUrl : `/${pathOrUrl}`
  return `${base}${path}`
}

export const getApiErrorMessage = (error, fallback = 'Operacja nie powiodla sie.') => {
  const data = error?.response?.data

  if (typeof data === 'string' && data.trim()) {
    return data
  }

  if (data?.message) {
    return data.message
  }

  if (typeof data?.detail === 'string' && data.detail.trim()) {
    return data.detail
  }

  if (data?.errors && typeof data.errors === 'object') {
    const firstError = Object.values(data.errors)
      .flat()
      .find((value) => typeof value === 'string' && value.trim())
    if (firstError) {
      return firstError
    }
  }

  if (data?.title) {
    return data.title
  }

  if (error?.message) {
    return error.message
  }

  return fallback
}

const apiClient = axios.create({
  baseURL: API_BASE_URL,
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

export { TOKEN_KEY }
export default apiClient
