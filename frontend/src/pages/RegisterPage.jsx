import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useLanguage } from '../context/LanguageContext'

const RegisterPage = () => {
  const { register } = useAuth()
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    email: '',
    password: '',
    role: 'Student',
  })
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
      await register(form.email, form.password, form.role)
      navigate('/login')
    } catch (err) {
      const backendErrors = err?.response?.data?.errors
      if (Array.isArray(backendErrors) && backendErrors.length > 0) {
        setError(backendErrors.join(' '))
      } else {
        setError(t('Nie udalo sie utworzyc konta. Sprawdz dane i sprobuj ponownie.'))
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page-shell auth-layout">
      <section className="card auth-card">
        <h1>{t('Rejestracja')}</h1>
        <p className="page-lead">{t('Utworz konto ucznia lub tutora i zacznij korzystac z platformy.')}</p>
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
            minLength={8}
          />

          <label htmlFor="role">{t('Rola')}</label>
          <select id="role" name="role" value={form.role} onChange={handleChange}>
            <option value="Student">{t('Student')}</option>
            <option value="Tutor">{t('Tutor')}</option>
          </select>

          {error && (
            <p className="error" role="alert">
              {error}
            </p>
          )}

          <button className="btn primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? t('Tworzenie konta...') : t('Zaloz konto')}
          </button>
        </form>
        <p className="muted">
          {t('Masz juz konto?')}{' '}
          <Link className="text-link" to="/login">
            {t('Zaloguj sie')}
          </Link>
        </p>
      </section>
    </main>
  )
}

export default RegisterPage
