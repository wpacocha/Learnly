import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const LoginPage = () => {
  const { login } = useAuth()
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
    } catch (_err) {
      setError('Nie udało się zalogować. Sprawdź dane i spróbuj ponownie.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page-shell auth-layout">
      <section className="card auth-card">
        <h1>Logowanie</h1>
        <p className="muted">Zaloguj się, aby zarządzać swoim kontem w Learnly.</p>
        <form onSubmit={handleSubmit} className="form">
          <label htmlFor="email">Email</label>
          <input
            id="email"
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            required
          />

          <label htmlFor="password">Hasło</label>
          <input
            id="password"
            name="password"
            type="password"
            value={form.password}
            onChange={handleChange}
            required
          />

          {error && <p className="error">{error}</p>}

          <button className="btn primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Logowanie...' : 'Zaloguj się'}
          </button>
        </form>
        <p className="muted">
          Nie masz konta? <Link to="/register">Zarejestruj się</Link>
        </p>
      </section>
    </main>
  )
}

export default LoginPage
