import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const AppLayout = () => {
  const { isAuthenticated, user, logout } = useAuth()

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
                <NavLink to="/dashboard">Panel</NavLink>
                <NavLink to="/search">Szukaj</NavLink>
                <NavLink to="/lessons">Lekcje</NavLink>
                {user.roles?.includes('Tutor') && (
                  <>
                    <NavLink to="/tutor/profile">Profil tutora</NavLink>
                  </>
                )}
                <span className="nav-divider" aria-hidden />
                <button type="button" className="btn nav-logout" onClick={logout}>
                  Wyloguj
                </button>
              </>
            ) : (
              <>
                <NavLink to="/login">Logowanie</NavLink>
                <NavLink to="/register">Rejestracja</NavLink>
              </>
            )}
          </nav>
        </div>
        {isAuthenticated && (
          <div className="topbar-user">
            Zalogowano jako <strong>{user.email}</strong>
          </div>
        )}
      </header>
      <Outlet />
    </>
  )
}

export default AppLayout
