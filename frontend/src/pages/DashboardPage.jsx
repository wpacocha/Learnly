import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const DashboardPage = () => {
  const { user } = useAuth()

  return (
    <main className="page-shell">
      <section className="card">
        <h1>Dashboard</h1>
        <p className="muted">Witaj ponownie w Learnly.</p>
        <div className="stack">
          <p>
            <strong>Email:</strong> {user.email}
          </p>
          <p>
            <strong>Role:</strong> {user.roles.join(', ')}
          </p>
        </div>
      </section>

      <section className="card">
        <h2>Szybkie akcje</h2>
        <div className="action-grid">
          <Link className="btn secondary" to="/search">
            Szukaj tutorów
          </Link>
          {user.roles.includes('Tutor') && (
            <Link className="btn secondary" to="/tutor/profile">
              Zarządzaj profilem tutora
            </Link>
          )}
        </div>
      </section>
    </main>
  )
}

export default DashboardPage
