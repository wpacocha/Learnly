import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const DashboardPage = () => {
  const { user } = useAuth()

  return (
    <main className="page-shell">
      <section className="card dashboard-welcome">
        <h1>Witaj w Learnly</h1>
        <p className="page-lead">
          Zarządzaj lekcjami i profilem z jednego miejsca. Wybierz poniżej, co chcesz zrobić.
        </p>
        <div className="tutor-meta-grid" style={{ marginTop: 12 }}>
          <p>
            <strong>Konto</strong>
            {user.email}
          </p>
          <p>
            <strong>Role</strong>
            {user.roles.join(', ')}
          </p>
        </div>
      </section>

      <section className="card">
        <h2>Szybkie przejścia</h2>
        <p className="muted">Najczęstsze akcje w jednym rzucie oka.</p>
        <div className="dashboard-actions">
          <Link className="link-card" to="/search">
            Znajdź tutora
            <span>Przeglądaj korepetytorów i filtruj wyniki</span>
          </Link>
          <Link className="link-card" to="/lessons">
            Moje lekcje
            <span>Lista rezerwacji i pokój lekcyjny</span>
          </Link>
          {user.roles.includes('Tutor') && (
            <>
              <Link className="link-card" to="/tutor/profile">
                Profil i oferta
                <span>Opis, przedmioty, poziomy nauczania</span>
              </Link>
              <Link className="link-card" to="/tutor/availability">
                Kalendarz dostępności
                <span>Dodaj sloty, które uczeń może zarezerwować</span>
              </Link>
            </>
          )}
        </div>
      </section>
    </main>
  )
}

export default DashboardPage
