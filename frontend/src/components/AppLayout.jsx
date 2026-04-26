import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const AppLayout = () => {
  const { isAuthenticated, user, logout } = useAuth()

  return (
    <>
      <header className="topbar">
        <div className="topbar-inner">
          <NavLink to="/" className="brand">
            Learnly
          </NavLink>
          <nav className="nav">
            {isAuthenticated ? (
              <>
                <NavLink to="/dashboard">Dashboard</NavLink>
                <NavLink to="/search">Szukaj tutorów</NavLink>
                <NavLink to="/tutor/profile">Profil tutora</NavLink>
                <button type="button" className="btn ghost" onClick={logout}>
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
