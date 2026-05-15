import { useEffect, useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { NOTIFICATIONS_CHANGED_EVENT, fetchNotifications } from '../api/notificationsApi'
import { useAuth } from '../context/AuthContext'
import { useLanguage } from '../context/LanguageContext'

const AppLayout = () => {
  const { isAuthenticated, user, logout } = useAuth()
  const { language, toggleLanguage, t } = useLanguage()
  const [unreadNotifications, setUnreadNotifications] = useState(0)

  useEffect(() => {
    if (!isAuthenticated) {
      return undefined
    }

    let cancelled = false
    const loadUnread = async () => {
      try {
        const data = await fetchNotifications()
        if (!cancelled) {
          setUnreadNotifications(Array.isArray(data) ? data.filter((x) => !x.readAtUtc).length : 0)
        }
      } catch {
        if (!cancelled) {
          setUnreadNotifications(0)
        }
      }
    }

    loadUnread()
    const id = window.setInterval(loadUnread, 10000)
    window.addEventListener(NOTIFICATIONS_CHANGED_EVENT, loadUnread)
    return () => {
      cancelled = true
      window.clearInterval(id)
      window.removeEventListener(NOTIFICATIONS_CHANGED_EVENT, loadUnread)
    }
  }, [isAuthenticated])

  return (
    <>
      <header className="topbar">
        <div className="topbar-inner">
          <NavLink to="/" className="brand" end>
            <span className="brand-mark" aria-hidden />
            Learnly
          </NavLink>
          <nav className="nav" aria-label="Główna nawigacja">
            {isAuthenticated ? (
              <>
                <NavLink to="/dashboard">{t('Panel')}</NavLink>
                <NavLink to="/search">{t('Szukaj')}</NavLink>
                <NavLink to="/conversations">{t('Chat')}</NavLink>
                <NavLink to="/notifications" className="nav-notification-link">
                  {t('Powiadomienia')}
                  {unreadNotifications > 0 && <span className="notification-badge">{unreadNotifications}</span>}
                </NavLink>
                <NavLink to="/lessons">{t('Lekcje')}</NavLink>
                {user.roles?.includes('Tutor') && (
                  <>
                    <NavLink to="/tutor/profile">{t('Profil tutora')}</NavLink>
                  </>
                )}
                <span className="nav-divider" aria-hidden />
                <button
                  type="button"
                  className="language-toggle"
                  onClick={toggleLanguage}
                  aria-label={language === 'pl' ? 'Switch to English' : 'Przełącz na polski'}
                >
                  <span aria-hidden>{language === 'pl' ? '🇵🇱' : '🇬🇧'}</span>
                  {language === 'pl' ? 'PL' : 'ENG'}
                </button>
                <button type="button" className="btn nav-logout" onClick={logout}>
                  {t('Wyloguj')}
                </button>
              </>
            ) : (
              <>
                <button
                  type="button"
                  className="language-toggle"
                  onClick={toggleLanguage}
                  aria-label={language === 'pl' ? 'Switch to English' : 'Przełącz na polski'}
                >
                  <span aria-hidden>{language === 'pl' ? '🇵🇱' : '🇬🇧'}</span>
                  {language === 'pl' ? 'PL' : 'ENG'}
                </button>
                <NavLink to="/login">{t('Logowanie')}</NavLink>
                <NavLink to="/register">{t('Rejestracja')}</NavLink>
              </>
            )}
          </nav>
        </div>
        {isAuthenticated && (
          <div className="topbar-user">
            {t('Zalogowano jako')} <strong>{user.email}</strong>
          </div>
        )}
      </header>
      <Outlet />
    </>
  )
}

export default AppLayout
