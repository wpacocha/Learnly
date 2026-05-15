import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getApiErrorMessage } from '../api/client'
import { NOTIFICATIONS_CHANGED_EVENT, fetchNotifications, markNotificationAsRead } from '../api/notificationsApi'
import { useLanguage } from '../context/LanguageContext'

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const NotificationsPage = () => {
  const { t } = useLanguage()
  const [notifications, setNotifications] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  const reload = useCallback(async () => {
    const data = await fetchNotifications()
    setNotifications(Array.isArray(data) ? data : [])
  }, [])

  useEffect(() => {
    let cancelled = false
    const refresh = async (showLoader = false) => {
      if (showLoader) {
        setIsLoading(true)
      }
      setError('')
      try {
        await reload()
      } catch (err) {
        if (!cancelled) {
          setError(getApiErrorMessage(err, 'Nie udalo sie pobrac powiadomien.'))
        }
      } finally {
        if (showLoader && !cancelled) {
          setIsLoading(false)
        }
      }
    }

    const load = async () => {
      await refresh(true)
    }
    load()
    const id = window.setInterval(() => refresh(false), 10000)
    const handleChanged = () => refresh(false)
    window.addEventListener(NOTIFICATIONS_CHANGED_EVENT, handleChanged)
    return () => {
      cancelled = true
      window.clearInterval(id)
      window.removeEventListener(NOTIFICATIONS_CHANGED_EVENT, handleChanged)
    }
  }, [reload])

  const markRead = async (id) => {
    try {
      setNotifications((current) =>
        current.map((notification) =>
          notification.id === id ? { ...notification, readAtUtc: new Date().toISOString() } : notification,
        ),
      )
      await markNotificationAsRead(id)
      await reload()
    } catch (err) {
      setError(getApiErrorMessage(err, 'Nie udalo sie oznaczyc powiadomienia jako przeczytane.'))
    }
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>{t('Ladowanie powiadomien...')}</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <h1>{t('Powiadomienia')}</h1>
        {error && <p className="error" role="alert">{error}</p>}
        {notifications.length === 0 && <p className="muted">{t('Brak powiadomien.')}</p>}
        <ul className="notification-list">
          {notifications.map((notification) => (
            <li key={notification.id} className={`notification-row${notification.readAtUtc ? '' : ' notification-row--unread'}`}>
              <div>
                <strong>{t(notification.message)}</strong>
                <p className="muted">{formatWhen(notification.createdAtUtc)}</p>
                {notification.linkUrl && (
                  <Link className="inline-link" to={notification.linkUrl}>
                    {t('Przejdz')}
                  </Link>
                )}
              </div>
              {!notification.readAtUtc && (
                <button className="btn secondary" type="button" onClick={() => markRead(notification.id)}>
                  {t('Oznacz jako przeczytane')}
                </button>
              )}
            </li>
          ))}
        </ul>
      </section>
    </main>
  )
}

export default NotificationsPage
