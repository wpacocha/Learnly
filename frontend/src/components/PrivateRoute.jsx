import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const PrivateRoute = ({ allowedRoles }) => {
  const { isAuthenticated, isLoading, user } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered">
          <p>Ładowanie sesji użytkownika...</p>
        </section>
      </main>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (
    allowedRoles?.length &&
    !allowedRoles.some((role) => (user.roles ?? []).includes(role))
  ) {
    return <Navigate to="/dashboard" replace />
  }

  return <Outlet />
}

export default PrivateRoute
