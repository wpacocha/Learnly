import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useLanguage } from '../context/LanguageContext'

const LoginPage = () => {
  const { login } = useAuth()
  const { t } = useLanguage()
  const navigate = useNavigate()
  const location = useLocation()
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleChange = (event) => {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      await login(form.email, form.password)
      const redirectPath = location.state?.from?.pathname ?? '/dashboard'
      navigate(redirectPath, { replace: true })
    } catch {
      setError(t('Nie udalo sie zalogowac. Sprawdz dane i sprobuj ponownie.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page-shell auth-layout">
      <section className="card auth-card">
        <h1>{t('Logowanie')}</h1>
        <p className="page-lead">{t('Zaloguj sie, aby korzystac z lekcji i profilu w Learnly.')}</p>
        <form onSubmit={handleSubmit} className="form">
          <label htmlFor="email">Email</label>
          <input id="email" name="email" type="email" value={form.email} onChange={handleChange} required />

          <label htmlFor="password">{t('Haslo')}</label>
          <input
            id="password"
            name="password"
            type="password"
            value={form.password}
            onChange={handleChange}
            required
          />

          {error && (
            <p className="error" role="alert">
              {error}
            </p>
          )}

          <button className="btn primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? t('Logowanie...') : t('Zaloguj sie')}
          </button>
        </form>
        <p className="muted">
          {t('Nie masz konta?')}{' '}
          <Link className="text-link" to="/register">
            {t('Zaloz konto')}
          </Link>
        </p>
      </section>
    </main>
  )
}

export default LoginPage
