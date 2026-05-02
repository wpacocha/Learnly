import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const RegisterPage = () => {
  const { register } = useAuth()
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
        setError('Nie udało się utworzyć konta. Sprawdź dane i spróbuj ponownie.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page-shell auth-layout">
      <section className="card auth-card">
        <h1>Rejestracja</h1>
        <p className="page-lead">Utwórz konto ucznia lub tutora i zacznij korzystać z platformy.</p>
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
            minLength={8}
          />

          <label htmlFor="role">Rola</label>
          <select id="role" name="role" value={form.role} onChange={handleChange}>
            <option value="Student">Student</option>
            <option value="Tutor">Tutor</option>
          </select>

          {error && (
            <p className="error" role="alert">
              {error}
            </p>
          )}

          <button className="btn primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Tworzenie konta...' : 'Załóż konto'}
          </button>
        </form>
        <p className="muted">
          Masz już konto?{' '}
          <Link className="text-link" to="/login">
            Zaloguj się
          </Link>
        </p>
      </section>
    </main>
  )
}

export default RegisterPage
