import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useLanguage } from '../context/LanguageContext'

const DashboardPage = () => {
  const { user } = useAuth()
  const { t } = useLanguage()

  return (
    <main className="page-shell">
      <section className="card dashboard-welcome">
        <h1>{t('Witaj w Learnly')}</h1>
        <p className="page-lead">
          {t('Zarządzaj lekcjami i profilem z jednego miejsca. Wybierz poniżej, co chcesz zrobić.')}
        </p>
        <div className="tutor-meta-grid" style={{ marginTop: 12 }}>
          <p>
            <strong>{t('Konto')}</strong>
            {user.email}
          </p>
          <p>
            <strong>{t('Role')}</strong>
            {user.roles.join(', ')}
          </p>
        </div>
      </section>

      <section className="card">
        <h2>{t('Szybkie przejścia')}</h2>
        <p className="muted">{t('Najczęstsze akcje w jednym rzucie oka.')}</p>
        <div className="dashboard-actions">
          <Link className="link-card" to="/search">
            {t('Znajdź tutora')}
            <span>{t('Przeglądaj korepetytorów i filtruj wyniki')}</span>
          </Link>
          <Link className="link-card" to="/lessons">
            {t('Moje lekcje')}
            <span>{t('Lista rezerwacji i pokój lekcyjny')}</span>
          </Link>
          {user.roles.includes('Tutor') && (
            <>
              <Link className="link-card" to="/tutor/profile">
                {t('Profil i oferta')}
                <span>{t('Opis, przedmioty, poziomy nauczania')}</span>
              </Link>
              <Link className="link-card" to="/tutor/availability">
                {t('Kalendarz dostępności')}
                <span>{t('Dodaj sloty, które uczeń może zarezerwować')}</span>
              </Link>
            </>
          )}
        </div>
      </section>
    </main>
  )
}

export default DashboardPage
